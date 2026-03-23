using Microsoft.AspNetCore.Mvc.RazorPages;

namespace SaleStore.Pages
{
    public class FavoritesModel : PageModel
    {
        public void OnGet()
        {
            ViewData["Title"] = "Sản Phẩm Yêu Thích - Cửa Hàng Cà Phê";
        }
    }
}
