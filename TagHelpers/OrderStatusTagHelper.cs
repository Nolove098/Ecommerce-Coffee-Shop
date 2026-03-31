using Microsoft.AspNetCore.Razor.TagHelpers;
using SaleStore.Models;

namespace SaleStore.TagHelpers
{
    /// <summary>
    /// Displays an order status badge.
    /// Usage: <order-status value="Pending" /> → renders a styled badge span
    /// </summary>
    [HtmlTargetElement("order-status", TagStructure = TagStructure.WithoutEndTag)]
    public class OrderStatusTagHelper : TagHelper
    {
        [HtmlAttributeName("value")]
        public OrderStatus Value { get; set; }

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            output.TagName = "span";
            output.TagMode = TagMode.StartTagAndEndTag;
            output.Attributes.SetAttribute("class", $"badge rounded-pill {Value.ToBadgeClass()}");
            output.Content.SetContent(Value.ToVietnamese());
        }
    }
}
