using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SaleStore.Data;

namespace SaleStore.Controllers;

public class MenuController : Controller
{
    private readonly ApplicationDbContext _context;

    public MenuController(ApplicationDbContext context)
    {
        _context = context;
    }

    // GET: /Menu
    [HttpGet]
    [Route("Menu")]
    public async Task<IActionResult> Index(string? category)
    {
        var products = await _context.Products
            .AsNoTracking()
            .Where(p => p.IsActive)
            .OrderBy(p => p.Category)
            .ThenBy(p => p.Name)
            .ToListAsync();

        ViewBag.Categories = products.Select(p => p.Category).Distinct().OrderBy(c => c).ToList();
        ViewBag.CurrentCategory = category;

        if (!string.IsNullOrWhiteSpace(category))
            products = products.Where(p => p.Category == category).ToList();

        return View(products);
    }
}
