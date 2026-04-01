using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using SaleStore.Data;
using SaleStore.Models;
using SaleStore.Models.ViewModels;

namespace SaleStore.Pages;

public class MenuModel : PageModel
{
    private readonly ApplicationDbContext _context;
    private const int PageSize = 12;

    public MenuModel(ApplicationDbContext context)
    {
        _context = context;
    }

    [BindProperty(SupportsGet = true)]
    public string? Category { get; set; }

    [BindProperty(SupportsGet = true)]
    public int Page { get; set; } = 1;

    public List<Product> Products { get; set; } = new();
    public List<CategoryInfo> Categories { get; set; } = new();
    public int CurrentPage { get; set; }
    public int TotalPages { get; set; }
    public int TotalProducts { get; set; }
    public string? SelectedCategory { get; set; }

    public async Task<IActionResult> OnGetAsync()
    {
        // Validate page parameter - redirect to page 1 if less than 1
        if (Page < 1)
        {
            return RedirectToPage("/Menu", new { category = Category, page = 1 });
        }

        // Populate category statistics
        Categories = await _context.Products
            .Where(p => p.IsActive)
            .GroupBy(p => p.Category)
            .Select(g => new CategoryInfo
            {
                Name = g.Key,
                ProductCount = g.Count()
            })
            .OrderBy(c => c.Name)
            .ToListAsync();

        // Build filtered query
        var query = _context.Products.Where(p => p.IsActive);

        // Apply category filter if specified
        if (!string.IsNullOrEmpty(Category))
        {
            // Check if category exists in the database
            var categoryExists = await _context.Products
                .AnyAsync(p => p.IsActive && p.Category == Category);
            
            if (categoryExists)
            {
                query = query.Where(p => p.Category == Category);
                SelectedCategory = Category;
            }
            // If category doesn't exist, show all products (no filter applied)
        }

        // Calculate total count and pages
        TotalProducts = await query.CountAsync();
        TotalPages = (int)Math.Ceiling(TotalProducts / (double)PageSize);

        // Validate page doesn't exceed total pages - redirect to last page if it does
        if (Page > TotalPages && TotalPages > 0)
        {
            return RedirectToPage("/Menu", new { category = Category, page = TotalPages });
        }

        // Set current page
        CurrentPage = Page;

        // Apply pagination with ordering
        Products = await query
            .OrderBy(p => p.Name)
            .Skip((Page - 1) * PageSize)
            .Take(PageSize)
            .ToListAsync();

        return Page();
    }
}
