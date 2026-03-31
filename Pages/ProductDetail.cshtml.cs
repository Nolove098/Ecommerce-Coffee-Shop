using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using SaleStore.Data;
using SaleStore.Models;

namespace SaleStore.Pages;

public class ProductDetailModel : PageModel
{
    private readonly ApplicationDbContext _context;

    public ProductDetailModel(ApplicationDbContext context)
    {
        _context = context;
    }

    public Product Product { get; set; } = null!;
    public List<Product> RelatedProducts { get; set; } = new();
    public List<Review> Reviews { get; set; } = new();
    public double AverageRating { get; set; }
    public int ReviewCount { get; set; }

    public async Task<IActionResult> OnGetAsync(int id)
    {
        var product = await _context.Products.FirstOrDefaultAsync(p => p.Id == id && p.IsActive);
        if (product == null)
            return RedirectToAction("Index", "Home");

        Product = product;

        RelatedProducts = await _context.Products
            .Where(p => p.IsActive && p.Category == product.Category && p.Id != product.Id)
            .OrderByDescending(p => p.Stock)
            .Take(4)
            .ToListAsync();

        Reviews = await _context.Reviews
            .Include(r => r.User)
            .Where(r => r.ProductId == id)
            .OrderByDescending(r => r.CreatedAt)
            .Take(20)
            .ToListAsync();

        ReviewCount = Reviews.Count;
        AverageRating = ReviewCount > 0 ? Reviews.Average(r => r.Rating) : 0;

        return Page();
    }
}
