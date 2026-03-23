using Microsoft.AspNetCore.Mvc;
using SaleStore.Data;
using SaleStore.Models;

namespace SaleStore.ViewComponents
{
    public class BestSellingProductsViewComponent : ViewComponent
    {
        private readonly ApplicationDbContext _context;

        public BestSellingProductsViewComponent(ApplicationDbContext context)
        {
            _context = context;
        }

        public IViewComponentResult Invoke()
        {
            // Get top 6 best-selling products (you can later add sales calculation logic)
            // For now, we'll get products sorted by ID (top rated/newest first)
            var products = _context.Products
                .Take(6)
                .OrderByDescending(p => p.Id)
                .ToList();

            return View(products);
        }
    }
}
