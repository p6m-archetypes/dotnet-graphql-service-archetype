using HotChocolate;
using HotChocolate.Types;
using {{ PrefixName }}{{ SuffixName }}.GraphQL.Types;
{% if persistence ~= 'None' %}
using Microsoft.EntityFrameworkCore;
using {{ PrefixName }}{{ SuffixName }}.Domain;
using {{ PrefixName }}{{ SuffixName }}.Resources;
{% endif %}
namespace {{ PrefixName }}{{ SuffixName }}.GraphQL;

{% if persistence ~= 'None' %}
// Reads over the persisted Item scaffold entity (Domain/Item.cs). Replace Item and these
// resolvers as your real domain lands. HotChocolate strips the Get prefix and camelCases:
//   Get{{ PrefixName }} -> {{ prefixName }}, Get{{ PrefixName }}s -> {{ prefixName }}s (the platform standard).
public class Query
{
    public string Health() => "OK";

    public async Task<{{ PrefixName }}Type?> Get{{ PrefixName }}(
        [GraphQLType(typeof(NonNullType<IdType>))] string id, [Service] AppDbContext db)
    {
        if (!Guid.TryParse(id, out var parsed)) return null;
        var item = await db.Items.FindAsync(parsed);
        return item is null ? null : {{ PrefixName }}Type.From(item);
    }

    public async Task<IEnumerable<{{ PrefixName }}Type>> Get{{ PrefixName }}s(
        [Service] AppDbContext db)
        => (await db.Items.OrderBy(i => i.CreatedAt).ToListAsync())
            .Select({{ PrefixName }}Type.From);
}
{% else %}
// In-memory stub resolvers — nothing is persisted. Select a persistence option to render the
// scaffold CRUD backed by a real database. HotChocolate strips the Get prefix and camelCases:
//   Get{{ PrefixName }} -> {{ prefixName }}, Get{{ PrefixName }}s -> {{ prefixName }}s (the platform standard).
public class Query
{
    public string Health() => "OK";

    public {{ PrefixName }}Type? Get{{ PrefixName }}([GraphQLType(typeof(NonNullType<IdType>))] string id)
        => new {{ PrefixName }}Type { Id = id, DisplayName = "" };

    public IEnumerable<{{ PrefixName }}Type> Get{{ PrefixName }}s()
        => [];
}
{% endif %}
