using HotChocolate;
using HotChocolate.Types;
using {{ PrefixName }}{{ SuffixName }}.GraphQL.Types;
{% if persistence ~= 'None' %}
using {{ PrefixName }}{{ SuffixName }}.Domain;
using {{ PrefixName }}{{ SuffixName }}.Resources;
{% endif %}
namespace {{ PrefixName }}{{ SuffixName }}.GraphQL;

{% if persistence ~= 'None' %}
// Writes over the persisted Item scaffold entity (Domain/Item.cs). Replace Item and these
// resolvers as your real domain lands. Mutations are entity-named per the platform standard:
//   create{{ PrefixName }} / update{{ PrefixName }} / delete{{ PrefixName }}.
public class Mutation
{
    public async Task<{{ PrefixName }}Type> Create{{ PrefixName }}(
        string displayName, [Service] AppDbContext db)
    {
        var item = new Item { Id = Guid.NewGuid(), DisplayName = displayName };
        db.Items.Add(item);
        await db.SaveChangesAsync();
        return {{ PrefixName }}Type.From(item);
    }

    public async Task<{{ PrefixName }}Type?> Update{{ PrefixName }}(
        [GraphQLType(typeof(NonNullType<IdType>))] string id, string displayName, [Service] AppDbContext db)
    {
        if (!Guid.TryParse(id, out var parsed)) return null;
        var item = await db.Items.FindAsync(parsed);
        if (item is null) return null;
        item.DisplayName = displayName;
        await db.SaveChangesAsync();
        return {{ PrefixName }}Type.From(item);
    }

    public async Task<bool> Delete{{ PrefixName }}(
        [GraphQLType(typeof(NonNullType<IdType>))] string id, [Service] AppDbContext db)
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
// scaffold CRUD backed by a real database. Mutations are entity-named per the platform standard:
//   create{{ PrefixName }} / update{{ PrefixName }} / delete{{ PrefixName }}.
public class Mutation
{
    public {{ PrefixName }}Type Create{{ PrefixName }}(string displayName)
        => new {{ PrefixName }}Type { Id = Guid.NewGuid().ToString(), DisplayName = displayName };

    public {{ PrefixName }}Type Update{{ PrefixName }}([GraphQLType(typeof(NonNullType<IdType>))] string id, string displayName)
        => new {{ PrefixName }}Type { Id = id, DisplayName = displayName };

    public bool Delete{{ PrefixName }}([GraphQLType(typeof(NonNullType<IdType>))] string id) => false;
}
{% endif %}
