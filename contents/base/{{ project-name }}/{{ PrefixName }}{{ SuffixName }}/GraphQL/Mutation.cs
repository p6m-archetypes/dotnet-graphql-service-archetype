using {{ PrefixName }}{{ SuffixName }}.GraphQL.Types;
{% if persistence ~= 'None' %}
using HotChocolate;
using {{ PrefixName }}{{ SuffixName }}.Domain;
using {{ PrefixName }}{{ SuffixName }}.Resources;
{% endif %}
namespace {{ PrefixName }}{{ SuffixName }}.GraphQL;

{% if persistence ~= 'None' %}
// Writes over the persisted Item scaffold entity (Domain/Item.cs). Replace Item and these
// resolvers as your real domain lands.
public class Mutation
{
    public async Task<{{ PrefixName }}{{ SuffixName }}Type> Create{{ PrefixName }}{{ SuffixName }}(
        string displayName, [Service] AppDbContext db)
    {
        var item = new Item { Id = Guid.NewGuid(), DisplayName = displayName };
        db.Items.Add(item);
        await db.SaveChangesAsync();
        return {{ PrefixName }}{{ SuffixName }}Type.From(item);
    }

    public async Task<{{ PrefixName }}{{ SuffixName }}Type?> Update{{ PrefixName }}{{ SuffixName }}(
        string id, string displayName, [Service] AppDbContext db)
    {
        if (!Guid.TryParse(id, out var parsed)) return null;
        var item = await db.Items.FindAsync(parsed);
        if (item is null) return null;
        item.DisplayName = displayName;
        await db.SaveChangesAsync();
        return {{ PrefixName }}{{ SuffixName }}Type.From(item);
    }

    public async Task<bool> Delete{{ PrefixName }}{{ SuffixName }}(
        string id, [Service] AppDbContext db)
    {
        if (!Guid.TryParse(id, out var parsed)) return false;
        var item = await db.Items.FindAsync(parsed);
        if (item is null) return false;
        db.Items.Remove(item);
        await db.SaveChangesAsync();
        return true;
    }
}
{% else %}
// In-memory stub resolvers — nothing is persisted. Select a persistence option to render the
// scaffold CRUD backed by a real database.
public class Mutation
{
    public {{ PrefixName }}{{ SuffixName }}Type Create{{ PrefixName }}{{ SuffixName }}(string displayName)
        => new {{ PrefixName }}{{ SuffixName }}Type { Id = Guid.NewGuid().ToString(), DisplayName = displayName };

    public {{ PrefixName }}{{ SuffixName }}Type Update{{ PrefixName }}{{ SuffixName }}(string id, string displayName)
        => new {{ PrefixName }}{{ SuffixName }}Type { Id = id, DisplayName = displayName };

    public bool Delete{{ PrefixName }}{{ SuffixName }}(string id) => false;
}
{% endif %}
