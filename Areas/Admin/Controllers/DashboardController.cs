using Microsoft.AspNetCore.Mvc;
using SaleStore.Data;
using SaleStore.Models;

namespace SaleStore.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class DashboardController : Controller
    {
        public IActionResult Index()
        {
            var products = MockDataStore.Products;
            var orders   = MockDataStore.Orders;

            ViewBag.TotalProducts    = products.Count;
            ViewBag.ActiveProducts   = products.Count(p => p.IsActive);
            ViewBag.TotalOrders      = orders.Count;
            ViewBag.PendingOrders    = orders.Count(o => o.Status == OrderStatus.Pending || o.Status == OrderStatus.Brewing);
            ViewBag.TodayRevenue     = orders
                .Where(o => o.Status == OrderStatus.Delivered && o.CreatedAt.Date == DateTime.Today)
                .Sum(o => o.Total);
            ViewBag.TotalRevenue     = orders
                .Where(o => o.Status == OrderStatus.Delivered)
                .Sum(o => o.Total);
            ViewBag.RecentOrders     = orders.OrderByDescending(o => o.CreatedAt).Take(5).ToList();

            return View();
        }
    }
}
