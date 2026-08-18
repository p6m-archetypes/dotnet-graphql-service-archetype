using HotChocolate;
using HotChocolate.Types;
{% if persistence ~= 'None' %}
using {{ ProjectName }}.Domain;
{% endif %}

namespace {{ ProjectName }}.GraphQL.Types;

// The entity as the platform standard names it: the GraphQL type is entity-named
// ("{{ EntityName }}", not service-named), fields `id`/`displayName`, id surfaced as ID.
[GraphQLName("{{ EntityName }}")]
public class {{ EntityName }}Type
{
    [GraphQLType(typeof(NonNullType<IdType>))]
    public string Id { get; init; } = string.Empty;

    public string DisplayName { get; init; } = string.Empty;
{% if persistence ~= 'None' %}

    public static {{ EntityName }}Type From(Item item)
        => new() { Id = item.Id.ToString(), DisplayName = item.DisplayName };
{% endif %}
}
