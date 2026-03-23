using Microsoft.AspNetCore.Mvc.RazorPages;
using SaleStore.Data;

namespace SaleStore.Pages
{
    public class MenuModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public List<SaleStore.Models.Product>? Products { get; set; }

        public MenuModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public void OnGet()
        {
            ViewData["Title"] = "Thực Đơn - Cửa Hàng Cà Phê";
            Products = _context.Products.OrderBy(p => p.Name).ToList();
        }
    }
}
