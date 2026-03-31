using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using SaleStore.Data;
using SaleStore.Models;

namespace SaleStore.Pages.Admin
{
    [Authorize(Roles = AppRoles.Admin)]
    public class ReportsModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public ReportsModel(ApplicationDbContext context)
        {
            _context = context;
        }

        [BindProperty(SupportsGet = true)]
        public DateTime? FromDate { get; set; }

        [BindProperty(SupportsGet = true)]
        public DateTime? ToDate { get; set; }

        public decimal TotalRevenue { get; set; }
        public int TotalOrders { get; set; }
        public int TotalItemsSold { get; set; }
        public decimal AverageOrderValue { get; set; }
        public List<DailyRevenueItem> DailyRevenue { get; set; } = new();
        public List<CategoryRevenueItem> CategoryRevenue { get; set; } = new();

        public async Task OnGetAsync()
        {
            var vnZone = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");
            var nowVn = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, vnZone);

            // Default: last 30 days
            if (!FromDate.HasValue) FromDate = nowVn.Date.AddDays(-30);
            if (!ToDate.HasValue) ToDate = nowVn.Date;

            var fromUtc = TimeZoneInfo.ConvertTimeToUtc(FromDate.Value.Date, vnZone);
            var toUtc = TimeZoneInfo.ConvertTimeToUtc(ToDate.Value.Date.AddDays(1), vnZone);

            var deliveredOrders = _context.Orders
                .Where(o => o.Status == OrderStatus.Delivered
                         && o.CreatedAt >= fromUtc
                         && o.CreatedAt < toUtc);

            TotalRevenue = await deliveredOrders.SumAsync(o => o.TotalAmount);
            TotalOrders = await deliveredOrders.CountAsync();
            AverageOrderValue = TotalOrders > 0 ? TotalRevenue / TotalOrders : 0;

            TotalItemsSold = await _context.OrderItems
                .Where(oi => oi.Order != null
                           && oi.Order.Status == OrderStatus.Delivered
                           && oi.Order.CreatedAt >= fromUtc
                           && oi.Order.CreatedAt < toUtc)
                .SumAsync(oi => oi.Quantity);

            // Daily revenue breakdown
            var rawDaily = await deliveredOrders
                .Select(o => new { o.CreatedAt, o.TotalAmount })
                .ToListAsync();

            DailyRevenue = rawDaily
                .GroupBy(o => TimeZoneInfo.ConvertTimeFromUtc(o.CreatedAt, vnZone).Date)
                .Select(g => new DailyRevenueItem
                {
                    Date = g.Key,
                    Revenue = g.Sum(x => x.TotalAmount),
                    OrderCount = g.Count()
                })
                .OrderBy(x => x.Date)
                .ToList();

            // Revenue by category
            CategoryRevenue = await _context.OrderItems
                .Where(oi => oi.Order != null
                           && oi.Order.Status == OrderStatus.Delivered
                           && oi.Order.CreatedAt >= fromUtc
                           && oi.Order.CreatedAt < toUtc
                           && oi.Product != null)
                .GroupBy(oi => oi.Product!.Category)
                .Select(g => new CategoryRevenueItem
                {
                    Category = g.Key,
                    Revenue = g.Sum(x => x.LineTotal),
                    ItemsSold = g.Sum(x => x.Quantity)
                })
                .OrderByDescending(x => x.Revenue)
                .ToListAsync();
        }

        public class DailyRevenueItem
        {
            public DateTime Date { get; set; }
            public decimal Revenue { get; set; }
            public int OrderCount { get; set; }
        }

        public class CategoryRevenueItem
        {
            public string Category { get; set; } = null!;
            public decimal Revenue { get; set; }
            public int ItemsSold { get; set; }
        }
    }
}
