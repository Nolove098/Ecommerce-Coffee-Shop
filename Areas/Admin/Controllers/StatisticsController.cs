using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SaleStore.Data;
using SaleStore.Models;

namespace SaleStore.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = AppRoles.Admin)]
    public class StatisticsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public StatisticsController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(string period = "month")
        {
            ViewBag.Period = period;

            var vnZone = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");
            var nowVn = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, vnZone);

            // Summary cards
            var deliveredOrders = _context.Orders.Where(o => o.Status == OrderStatus.Delivered);
            ViewBag.TotalRevenue = await deliveredOrders.SumAsync(o => o.TotalAmount);
            ViewBag.TotalOrders = await _context.Orders.CountAsync();
            ViewBag.DeliveredOrders = await deliveredOrders.CountAsync();
            ViewBag.TotalCustomers = await _context.Customers.CountAsync();

            // Top products by quantity sold
            var topProducts = await _context.OrderItems
                .Include(oi => oi.Product)
                .Where(oi => oi.Order!.Status == OrderStatus.Delivered)
                .GroupBy(oi => new { oi.ProductId, oi.Product!.Name })
                .Select(g => new
                {
                    ProductName = g.Key.Name,
                    TotalQuantity = g.Sum(x => x.Quantity),
                    TotalRevenue = g.Sum(x => x.UnitPrice * x.Quantity)
                })
                .OrderByDescending(x => x.TotalQuantity)
                .Take(10)
                .ToListAsync();

            ViewBag.TopProducts = topProducts.Select(p => new TopProductDto
            {
                ProductName = p.ProductName,
                TotalQuantity = p.TotalQuantity,
                TotalRevenue = p.TotalRevenue
            }).ToList();

            return View();
        }

        [HttpGet]
        public async Task<IActionResult> ChartData(string period = "month")
        {
            var vnZone = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");
            var nowVn = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, vnZone);

            var deliveredOrders = await _context.Orders
                .Where(o => o.Status == OrderStatus.Delivered)
                .Select(o => new { o.CreatedAt, o.TotalAmount })
                .ToListAsync();

            // Convert to VN time
            var ordersVn = deliveredOrders.Select(o => new
            {
                CreatedAtVn = TimeZoneInfo.ConvertTimeFromUtc(o.CreatedAt, vnZone),
                o.TotalAmount
            }).ToList();

            List<string> labels;
            List<decimal> revenues;
            List<int> counts;

            switch (period)
            {
                case "week":
                    // Last 7 days
                    var startWeek = nowVn.Date.AddDays(-6);
                    labels = Enumerable.Range(0, 7).Select(i => startWeek.AddDays(i).ToString("dd/MM")).ToList();
                    revenues = Enumerable.Range(0, 7).Select(i =>
                    {
                        var day = startWeek.AddDays(i).Date;
                        return ordersVn.Where(o => o.CreatedAtVn.Date == day).Sum(o => o.TotalAmount);
                    }).ToList();
                    counts = Enumerable.Range(0, 7).Select(i =>
                    {
                        var day = startWeek.AddDays(i).Date;
                        return ordersVn.Count(o => o.CreatedAtVn.Date == day);
                    }).ToList();
                    break;

                case "year":
                    // Last 12 months
                    labels = Enumerable.Range(0, 12).Select(i =>
                    {
                        var m = nowVn.AddMonths(-11 + i);
                        return $"T{m.Month}/{m.Year % 100}";
                    }).ToList();
                    revenues = Enumerable.Range(0, 12).Select(i =>
                    {
                        var m = nowVn.AddMonths(-11 + i);
                        return ordersVn.Where(o => o.CreatedAtVn.Year == m.Year && o.CreatedAtVn.Month == m.Month).Sum(o => o.TotalAmount);
                    }).ToList();
                    counts = Enumerable.Range(0, 12).Select(i =>
                    {
                        var m = nowVn.AddMonths(-11 + i);
                        return ordersVn.Count(o => o.CreatedAtVn.Year == m.Year && o.CreatedAtVn.Month == m.Month);
                    }).ToList();
                    break;

                default: // month — last 30 days
                    var startMonth = nowVn.Date.AddDays(-29);
                    labels = Enumerable.Range(0, 30).Select(i => startMonth.AddDays(i).ToString("dd/MM")).ToList();
                    revenues = Enumerable.Range(0, 30).Select(i =>
                    {
                        var day = startMonth.AddDays(i).Date;
                        return ordersVn.Where(o => o.CreatedAtVn.Date == day).Sum(o => o.TotalAmount);
                    }).ToList();
                    counts = Enumerable.Range(0, 30).Select(i =>
                    {
                        var day = startMonth.AddDays(i).Date;
                        return ordersVn.Count(o => o.CreatedAtVn.Date == day);
                    }).ToList();
                    break;
            }

            return Json(new { labels, revenues, counts });
        }
    }

    public class TopProductDto
    {
        public string ProductName { get; set; } = null!;
        public int TotalQuantity { get; set; }
        public decimal TotalRevenue { get; set; }
    }
}
