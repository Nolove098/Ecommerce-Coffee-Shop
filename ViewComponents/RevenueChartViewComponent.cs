using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SaleStore.Data;
using SaleStore.Models;

namespace SaleStore.ViewComponents;

public class RevenueChartViewComponent : ViewComponent
{
    private readonly ApplicationDbContext _dbContext;

    public RevenueChartViewComponent(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IViewComponentResult> InvokeAsync()
    {
        var vnZone = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");
        var nowVn = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, vnZone);
        var todayVn = nowVn.Date;
        var todayUtc = TimeZoneInfo.ConvertTimeToUtc(todayVn, vnZone);

        // Query delivered orders today, grouped by 2-hour slots
        var todayOrders = await _dbContext.Orders
            .Where(o => o.Status == OrderStatus.Delivered && o.CreatedAt >= todayUtc)
            .Select(o => new { o.CreatedAt, o.TotalAmount })
            .ToListAsync();

        // Build time slots: 6AM, 8AM, 10AM, 12PM, 2PM, 4PM, 6PM, 8PM
        var slots = new[] { 6, 8, 10, 12, 14, 16, 18, 20 };
        var labels = new[] { "6 AM", "8 AM", "10 AM", "12 PM", "2 PM", "4 PM", "6 PM", "8 PM" };

        var values = slots.Select(hour =>
        {
            var slotStart = todayUtc.AddHours(hour);
            var slotEnd = todayUtc.AddHours(hour + 2);
            return todayOrders
                .Where(o => o.CreatedAt >= slotStart && o.CreatedAt < slotEnd)
                .Sum(o => o.TotalAmount);
        }).ToArray();

        var totalToday = values.Sum();

        ViewBag.Labels = labels;
        ViewBag.Values = values;
        ViewBag.TotalToday = totalToday;

        return View();
    }
}
