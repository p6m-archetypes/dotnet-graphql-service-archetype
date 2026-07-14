{% if persistence ~= 'None' %}
using {{ PrefixName }}{{ SuffixName }}.Domain;

{% endif %}
namespace {{ PrefixName }}{{ SuffixName }}.GraphQL.Types;

public class {{ PrefixName }}{{ SuffixName }}Type
{
    public string Id { get; init; } = string.Empty;
    public string DisplayName { get; init; } = string.Empty;
{% if persistence ~= 'None' %}

    public static {{ PrefixName }}{{ SuffixName }}Type From(Item item)
        => new() { Id = item.Id.ToString(), DisplayName = item.DisplayName };
{% endif %}
}
