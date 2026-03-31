using Microsoft.AspNetCore.Razor.TagHelpers;

namespace SaleStore.TagHelpers
{
    /// <summary>
    /// Displays a formatted VND currency value.
    /// Usage: <currency value="123000" /> → renders "123,000 ₫"
    /// </summary>
    [HtmlTargetElement("currency", TagStructure = TagStructure.WithoutEndTag)]
    public class CurrencyTagHelper : TagHelper
    {
        [HtmlAttributeName("value")]
        public decimal Value { get; set; }

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            output.TagName = "span";
            output.TagMode = TagMode.StartTagAndEndTag;
            output.Attributes.SetAttribute("class", "fw-bold text-coffee");
            output.Content.SetContent($"{Value:N0} ₫");
        }
    }
}
