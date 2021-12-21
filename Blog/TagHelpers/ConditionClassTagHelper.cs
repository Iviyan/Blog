using Microsoft.AspNetCore.Razor.TagHelpers;

namespace Blog.TagHelpers;

[HtmlTargetElement(Attributes = ClassValuesPrefix + "*")]
public class ConditionClassTagHelper : TagHelper
{
    private const string ClassValuesPrefix = "class-";

    private IDictionary<string, bool> classValues = default!;

    [HtmlAttributeName(DictionaryAttributePrefix = ClassValuesPrefix)]
    public IDictionary<string, bool> ClassValues
    {
        get => classValues ??= new Dictionary<string, bool>(StringComparer.OrdinalIgnoreCase);
        set => classValues = value;
    }

    public override void Process(TagHelperContext context, TagHelperOutput output)
    {
        string classList = String.Join(" ",
            ClassValues.Where(e => e.Value).Select(e => e.Key));

        if (classList.Length > 0)
        {
            var classValue = context.AllAttributes.TryGetAttribute("class", out var attr)
                ? $"{attr.Value} {classList}" : classList;
            output.Attributes.SetAttribute("class", classValue);
        }

    }
}