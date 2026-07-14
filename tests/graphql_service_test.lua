--- Acceptance suite for the .NET GraphQL service archetype: renders each persistence variant,
--- verifies the layout, builds it, boots it against a real database container, and proves GraphQL
--- CRUD operations round-trip into that database.
---
--- Run from the archetype repo root (uses ./prova.toml):   prova
--- requires docker + dotnet (SDK 9); skips cleanly without them.

local SRC = "."

local BASE_ANSWERS = {
  author_name     = "Test Author",
  author_email    = "test@example.com",
  org_name        = "acme",
  solution_name   = "platform",
  prefix_name     = "Example",
  suffix_name     = "Service",
  image_registry  = "ghcr.io/acme",
}

local function answers_with(extra)
  local out = {}
  for k, v in pairs(BASE_ANSWERS) do out[k] = v end
  for k, v in pairs(extra) do out[k] = v end
  return out
end

-- HotChocolate camelCases resolvers and strips the Get prefix:
--   GetExampleService -> exampleService, ListExampleServices -> listExampleServices, etc.
local CREATE = [[mutation($name: String!) { createExampleService(displayName: $name) { id displayName } }]]
local GET    = [[query($id: String!) { exampleService(id: $id) { id displayName } }]]
local LIST   = [[{ listExampleServices { id displayName } }]]
local UPDATE = [[mutation($id: String!, $name: String!) { updateExampleService(id: $id, displayName: $name) { id displayName } }]]
local DELETE = [[mutation($id: String!) { deleteExampleService(id: $id) }]]

local SCAFFOLD_FILES = {
  "ExampleService/Resources/Persistence.cs",
  "ExampleService/Resources/Persistence.Entities.cs",
  "ExampleService/Domain/Item.cs",
}

local VARIANTS = {
  {
    persistence = "PostgreSQL",
    db = postgres,
    count_by_name = [[SELECT count(*) FROM "Items" WHERE "DisplayName" = $1]],
  },
  {
    persistence = "MySQL",
    db = mysql,
    count_by_name = "SELECT count(*) FROM `Items` WHERE `DisplayName` = ?",
  },
}

for _, v in ipairs(VARIANTS) do
  local label = "dotnet-graphql[" .. v.persistence .. "]"

  local project = prova.fixture(label .. ":project", Scope.File, function(ctx)
    return archetect.render{
      source = SRC,
      answers = answers_with{ persistence = v.persistence },
      destination = ctx:tempdir(),
      defaults = true,
    }
  end)

  archetect.verify(project, {
    name = label,
    project_dir = "example-service",
    expected_files = {
      "ExampleService.sln",
      "ExampleService/Program.cs",
      "ExampleService/GraphQL/Query.cs",
      "ExampleService/GraphQL/Mutation.cs",
      SCAFFOLD_FILES[1], SCAFFOLD_FILES[2], SCAFFOLD_FILES[3],
      ".github/workflows/build.yaml",
    },
    yaml_globs = { ".platform/kubernetes/**/*.yaml" },
    requires = { "dotnet" },
    build_steps = { "dotnet build ExampleService.sln" },
  })

  local service = prova.fixture(label .. ":service", Scope.File, function(ctx)
    local root = ctx:use(project):dir("example-service")
    local db = v.db.container(ctx)

    local build = shell.run("dotnet build ExampleService.sln -c Release", {
      cwd = root.path, timeout = "600s",
    })
    assert(build:ok(), label .. " failed to build:\n" .. build.stderr .. build.stdout)

    local port, mgmt = net.free_port(), net.free_port()
    ctx:manage(shell.spawn("dotnet ExampleService.dll", {
      cwd = root.path .. "/ExampleService/bin/Release/net9.0",
      env = {
        Port           = tostring(port),
        ManagementPort = tostring(mgmt),
        DbHost         = "127.0.0.1",
        DbPort         = tostring(db.container:host_port(v.persistence == "MySQL" and 3306 or 5432)),
        DbUsername     = "prova",
        DbPassword     = "prova",
        DbDbname       = "prova",
      },
    }))

    local api = graphql.client{ url = "http://127.0.0.1:" .. port .. "/graphql" }
    -- The health field answering proves boot completed — including EnsureCreated against the DB.
    prova.retry(function() return api:query("{ health }") end,
      { timeout = "60s", message = label .. " graphql endpoint never became ready" })
    return { api = api, db = db.client }
  end)

  prova.group(label .. " CRUD round-trip", { requires = { "docker", "dotnet" } }, function(g)
    g:test("created entities land in " .. v.persistence, function(t)
      local svc = t:use(service)

      local created = svc.api:query(CREATE, { name = "widget" }).createExampleService
      t:expect(created.displayName):equals("widget")
      t:expect(created.id, "created id"):is_truthy()

      t:expect(svc.db:query_value(v.count_by_name, { "widget" }), "rows in DB"):equals(1)

      -- Read back through the API (the old stub echoed the id with an empty name).
      local fetched = svc.api:query(GET, { id = created.id }).exampleService
      t:expect(fetched.displayName):equals("widget")

      local listed = svc.api:query(LIST).listExampleServices
      local found = false
      for _, e in ipairs(listed or {}) do
        if e.id == created.id then found = true end
      end
      t:expect(found, "created entity present in listExampleServices"):is_true()
    end)

    g:test("updates and deletes round-trip into " .. v.persistence, function(t)
      local svc = t:use(service)

      local created = svc.api:query(CREATE, { name = "ephemeral" }).createExampleService

      local updated = svc.api:query(UPDATE, { id = created.id, name = "renamed" }).updateExampleService
      t:expect(updated.displayName):equals("renamed")
      t:expect(svc.db:query_value(v.count_by_name, { "renamed" }), "renamed row in DB"):equals(1)
      t:expect(svc.db:query_value(v.count_by_name, { "ephemeral" }), "old name gone"):equals(0)

      t:expect(svc.api:query(DELETE, { id = created.id }).deleteExampleService, "delete reports true"):is_true()
      local gone = svc.api:query(GET, { id = created.id }).exampleService
      t:expect(gone):is_nil()
      t:expect(svc.db:query_value(v.count_by_name, { "renamed" }), "row deleted from DB"):equals(0)
    end)
  end)
end

-- The hollow rendering stays hollow: stub resolvers, no scaffold files.
archetect.verify{
  name = "dotnet-graphql[None]",
  source = SRC,
  answers = answers_with{ persistence = "None" },
  project_dir = "example-service",
  expected_files = {
    "ExampleService.sln",
    "ExampleService/Program.cs",
    "ExampleService/GraphQL/Query.cs",
  },
  absent_files = SCAFFOLD_FILES,
  requires = { "dotnet" },
  build_steps = { "dotnet build ExampleService.sln" },
}
