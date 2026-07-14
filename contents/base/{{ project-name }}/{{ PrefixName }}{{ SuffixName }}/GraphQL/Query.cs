using {{ PrefixName }}{{ SuffixName }}.GraphQL.Types;
{% if persistence ~= 'None' %}
using HotChocolate;
using Microsoft.EntityFrameworkCore;
using {{ PrefixName }}{{ SuffixName }}.Domain;
using {{ PrefixName }}{{ SuffixName }}.Resources;
{% endif %}
namespace {{ PrefixName }}{{ SuffixName }}.GraphQL;

{% if persistence ~= 'None' %}
// Reads over the persisted Item scaffold entity (Domain/Item.cs). Replace Item and these
// resolvers as your real domain lands.
public class Query
{
    public string Health() => "OK";

    public async Task<{{ PrefixName }}{{ SuffixName }}Type?> Get{{ PrefixName }}{{ SuffixName }}(
        string id, [Service] AppDbContext db)
    {
        if (!Guid.TryParse(id, out var parsed)) return null;
        var item = await db.Items.FindAsync(parsed);
        return item is null ? null : {{ PrefixName }}{{ SuffixName }}Type.From(item);
    }

    public async Task<IEnumerable<{{ PrefixName }}{{ SuffixName }}Type>> List{{ PrefixName }}{{ SuffixName }}s(
        [Service] AppDbContext db)
        => (await db.Items.OrderBy(i => i.CreatedAt).ToListAsync())
            .Select({{ PrefixName }}{{ SuffixName }}Type.From);
}
{% else %}
// In-memory stub resolvers — nothing is persisted. Select a persistence option to render the
// scaffold CRUD backed by a real database.
public class Query
{
    public string Health() => "OK";

    public {{ PrefixName }}{{ SuffixName }}Type? Get{{ PrefixName }}{{ SuffixName }}(string id)
        => new {{ PrefixName }}{{ SuffixName }}Type { Id = id, DisplayName = "" };

    public IEnumerable<{{ PrefixName }}{{ SuffixName }}Type> List{{ PrefixName }}{{ SuffixName }}s()
        => [];
}
{% endif %}
