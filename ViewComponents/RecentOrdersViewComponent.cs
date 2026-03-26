using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SaleStore.Data;

namespace SaleStore.ViewComponents;

public class RecentOrdersViewComponent : ViewComponent
{
    private readonly ApplicationDbContext _dbContext;

    public RecentOrdersViewComponent(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IViewComponentResult> InvokeAsync(int count = 5)
    {
        var orders = await _dbContext.Orders
            .Include(o => o.Customer)
            .OrderByDescending(o => o.CreatedAt)
            .Take(count)
            .ToListAsync();

        return View(orders);
    }
}
