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

    public async Task<IActionResult> OnGetAsync(int id)
    {
        var product = await _context.Products.FirstOrDefaultAsync(p => p.Id == id && p.IsActive);
        if (product == null)
            return RedirectToAction("Index", "Home");

        Product = product;

        // Lấy sản phẩm cùng danh mục
        RelatedProducts = await _context.Products
            .Where(p => p.IsActive && p.Category == product.Category && p.Id != product.Id)
            .OrderByDescending(p => p.Stock)
            .Take(4)
            .ToListAsync();

        return Page();
    }
}
