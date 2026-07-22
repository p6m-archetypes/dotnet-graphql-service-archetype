using HotChocolate;
using HotChocolate.Types;
{% if persistence ~= 'None' %}
using {{ PrefixName }}{{ SuffixName }}.Domain;
{% endif %}

namespace {{ PrefixName }}{{ SuffixName }}.GraphQL.Types;

// The entity as the platform standard names it: the GraphQL type is entity-named
// ("{{ PrefixName }}", not service-named), fields `id`/`displayName`, id surfaced as ID.
[GraphQLName("{{ PrefixName }}")]
public class {{ PrefixName }}Type
{
    [GraphQLType(typeof(NonNullType<IdType>))]
    public string Id { get; init; } = string.Empty;

    public string DisplayName { get; init; } = string.Empty;
{% if persistence ~= 'None' %}

    public static {{ PrefixName }}Type From(Item item)
        => new() { Id = item.Id.ToString(), DisplayName = item.DisplayName };
{% endif %}
}
