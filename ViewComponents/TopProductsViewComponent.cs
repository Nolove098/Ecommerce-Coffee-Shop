using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SaleStore.Data;
using SaleStore.Models;

namespace SaleStore.ViewComponents;

public class TopProductsViewComponent : ViewComponent
{
    private readonly ApplicationDbContext _dbContext;

    public TopProductsViewComponent(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IViewComponentResult> InvokeAsync(int count = 5)
    {
        var topProducts = await _dbContext.OrderItems
            .Where(oi => oi.Order != null && oi.Order.Status == OrderStatus.Delivered)
            .GroupBy(oi => oi.ProductName)
            .Select(g => new TopProductViewModel
            {
                Name = g.Key,
                TotalQuantity = g.Sum(x => x.Quantity),
                TotalRevenue = g.Sum(x => x.LineTotal)
            })
            .OrderByDescending(x => x.TotalQuantity)
            .Take(count)
            .ToListAsync();

        return View(topProducts);
    }
}

public class TopProductViewModel
{
    public string Name { get; set; } = null!;
    public int TotalQuantity { get; set; }
    public decimal TotalRevenue { get; set; }
}
