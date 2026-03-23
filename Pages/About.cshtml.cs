using Microsoft.AspNetCore.Mvc.RazorPages;

namespace SaleStore.Pages
{
    public class AboutModel : PageModel
    {
        public void OnGet()
        {
            ViewData["Title"] = "Giới thiệu - Cửa hàng Cà phê";
        }
    }
}
