using HotChocolate;
using HotChocolate.Types;
using {{ ProjectName }}.GraphQL.Types;
{% if persistence ~= 'None' %}
using {{ ProjectName }}.Domain;
using {{ ProjectName }}.Resources;
{% endif %}
namespace {{ ProjectName }}.GraphQL;

{% if persistence ~= 'None' %}
// Writes over the persisted Item scaffold entity (Domain/Item.cs). Replace Item and these
// resolvers as your real domain lands. Mutations are entity-named per the platform standard:
//   create{{ EntityName }} / update{{ EntityName }} / delete{{ EntityName }}.
public class Mutation
{
    public async Task<{{ EntityName }}Type> Create{{ EntityName }}(
        string displayName, [Service] AppDbContext db)
    {
        var item = new Item { Id = Guid.NewGuid(), DisplayName = displayName };
        db.Items.Add(item);
        await db.SaveChangesAsync();
        return {{ EntityName }}Type.From(item);
    }

    public async Task<{{ EntityName }}Type?> Update{{ EntityName }}(
        [GraphQLType(typeof(NonNullType<IdType>))] string id, string displayName, [Service] AppDbContext db)
    {
        if (!Guid.TryParse(id, out var parsed)) return null;
        var item = await db.Items.FindAsync(parsed);
        if (item is null) return null;
        item.DisplayName = displayName;
        await db.SaveChangesAsync();
        return {{ EntityName }}Type.From(item);
    }

    public async Task<bool> Delete{{ EntityName }}(
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
//   create{{ EntityName }} / update{{ EntityName }} / delete{{ EntityName }}.
public class Mutation
{
    public {{ EntityName }}Type Create{{ EntityName }}(string displayName)
        => new {{ EntityName }}Type { Id = Guid.NewGuid().ToString(), DisplayName = displayName };

    public {{ EntityName }}Type Update{{ EntityName }}([GraphQLType(typeof(NonNullType<IdType>))] string id, string displayName)
        => new {{ EntityName }}Type { Id = id, DisplayName = displayName };

    public bool Delete{{ EntityName }}([GraphQLType(typeof(NonNullType<IdType>))] string id) => false;
}
{% endif %}
