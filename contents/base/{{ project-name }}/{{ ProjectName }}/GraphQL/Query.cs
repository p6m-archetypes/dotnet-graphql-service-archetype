using HotChocolate;
using HotChocolate.Types;
using {{ ProjectName }}.GraphQL.Types;
{% if persistence ~= 'None' %}
using Microsoft.EntityFrameworkCore;
using {{ ProjectName }}.Domain;
using {{ ProjectName }}.Resources;
{% endif %}
namespace {{ ProjectName }}.GraphQL;

{% if persistence ~= 'None' %}
// Reads over the persisted {{ EntityName }} scaffold entity (Domain/{{ EntityName }}.cs). Replace {{ EntityName }} and these
// resolvers as your real domain lands. HotChocolate strips the Get prefix and camelCases:
//   Get{{ EntityName }} -> {{ entityName }}, Get{{ EntityName }}s -> {{ entityName }}s (the platform standard).
public class Query
{
    public string Health() => "OK";

    public async Task<{{ EntityName }}Type?> Get{{ EntityName }}(
        [GraphQLType(typeof(NonNullType<IdType>))] string id, [Service] AppDbContext db)
    {
        if (!Guid.TryParse(id, out var parsed)) return null;
        var item = await db.{{ EntityName }}s.FindAsync(parsed);
        return item is null ? null : {{ EntityName }}Type.From(item);
    }

    public async Task<IEnumerable<{{ EntityName }}Type>> Get{{ EntityName }}s(
        [Service] AppDbContext db)
        => (await db.{{ EntityName }}s.OrderBy(i => i.CreatedAt).ToListAsync())
            .Select({{ EntityName }}Type.From);
}
{% else %}
// In-memory stub resolvers — nothing is persisted. Select a persistence option to render the
// scaffold CRUD backed by a real database. HotChocolate strips the Get prefix and camelCases:
//   Get{{ EntityName }} -> {{ entityName }}, Get{{ EntityName }}s -> {{ entityName }}s (the platform standard).
public class Query
{
    public string Health() => "OK";

    public {{ EntityName }}Type? Get{{ EntityName }}([GraphQLType(typeof(NonNullType<IdType>))] string id)
        => new {{ EntityName }}Type { Id = id, DisplayName = "" };

    public IEnumerable<{{ EntityName }}Type> Get{{ EntityName }}s()
        => [];
}
{% endif %}
