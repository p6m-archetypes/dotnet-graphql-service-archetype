using {{ PrefixName }}{{ SuffixName }}.GraphQL.Types;
{% if persistence ~= 'None' %}
using {{ PrefixName }}{{ SuffixName }}.Resources;
{% endif %}
namespace {{ PrefixName }}{{ SuffixName }}.GraphQL;

public class Mutation
{
    public {{ PrefixName }}{{ SuffixName }}Type Create{{ PrefixName }}{{ SuffixName }}(
        string displayName{% if persistence ~= 'None' %}, [Service] AppDbContext? db = null{% endif %})
        => new {{ PrefixName }}{{ SuffixName }}Type { Id = Guid.NewGuid().ToString(), DisplayName = displayName };

    public {{ PrefixName }}{{ SuffixName }}Type Update{{ PrefixName }}{{ SuffixName }}(
        string id, string displayName{% if persistence ~= 'None' %}, [Service] AppDbContext? db = null{% endif %})
        => new {{ PrefixName }}{{ SuffixName }}Type { Id = id, DisplayName = displayName };
}
