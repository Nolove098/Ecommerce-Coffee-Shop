using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using SaleStore.Data;
using SaleStore.Models;

namespace SaleStore.Pages;

public class PopularModel : PageModel
{
    private readonly ApplicationDbContext _context;

    public PopularModel(ApplicationDbContext context)
    {
        _context = context;
    }

    public List<Product> PopularProducts { get; set; } = new();

    public async Task OnGetAsync()
    {
        // Lấy 5 sản phẩm đầu tiên từ database
        PopularProducts = await _context.Products
            .Where(p => p.IsActive)
            .OrderByDescending(p => p.Stock) // Sắp xếp theo stock để giả lập "bán chạy"
            .Take(5)
            .ToListAsync();
    }
}
