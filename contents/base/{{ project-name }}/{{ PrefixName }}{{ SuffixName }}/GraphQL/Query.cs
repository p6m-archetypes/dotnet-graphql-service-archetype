using {{ PrefixName }}{{ SuffixName }}.GraphQL.Types;
{% if persistence ~= 'None' %}
using {{ PrefixName }}{{ SuffixName }}.Resources;
{% endif %}
namespace {{ PrefixName }}{{ SuffixName }}.GraphQL;

public class Query
{
    public string Health() => "OK";

    public {{ PrefixName }}{{ SuffixName }}Type? Get{{ PrefixName }}{{ SuffixName }}(
        string id{% if persistence ~= 'None' %}, [Service] AppDbContext? db = null{% endif %})
        => new {{ PrefixName }}{{ SuffixName }}Type { Id = id, DisplayName = "" };

    public IEnumerable<{{ PrefixName }}{{ SuffixName }}Type> List{{ PrefixName }}{{ SuffixName }}s(
        {% if persistence ~= 'None' %}[Service] AppDbContext? db = null{% endif %})
        => [];
}
