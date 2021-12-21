using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace Blog.TagHelpers;

[HtmlTargetElement("input", Attributes = ValidateAttributeName)]
public class BootstrapValidateTagHelper : TagHelper
{
    private const string ValidateAttributeName = "bs-valid";

    private const string IsValidClassName = "is-valid";
    private const string IsInvalidClassName = "is-invalid";

    [ViewContext, HtmlAttributeNotBound]
    public ViewContext ViewContext { get; set; } = default!;

    public override void Process(TagHelperContext context, TagHelperOutput output)
    {
        if (!context.AllAttributes.TryGetAttribute("asp-for", out var modelProperty)) return;
        string modelPropertyName = ((ModelExpression)modelProperty.Value).Name;

        if (!ViewContext.ModelState.TryGetValue(modelPropertyName!, out var modelState)) return;

        var bsClass = modelState.Errors.Count > 0 ? IsInvalidClassName : IsValidClassName;
        if (context.AllAttributes.TryGetAttribute("class", out var classAttribute))
            output.Attributes.SetAttribute("class", $"{classAttribute.Value} {bsClass}");
        else
            output.Attributes.Add("class", bsClass);
    }
}
