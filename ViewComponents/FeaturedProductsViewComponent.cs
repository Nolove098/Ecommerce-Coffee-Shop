using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SaleStore.Data;

namespace SaleStore.ViewComponents;

public class FeaturedProductsViewComponent : ViewComponent
{
    private readonly ApplicationDbContext _dbContext;

    public FeaturedProductsViewComponent(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IViewComponentResult> InvokeAsync(int count = 4)
    {
        var products = await _dbContext.Products
            .Where(p => p.IsActive)
            .OrderByDescending(p => p.Stock)
            .ThenBy(p => p.Name)
            .Take(count)
            .ToListAsync();

        return View(products);
    }
}
