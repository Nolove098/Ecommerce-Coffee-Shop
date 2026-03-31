using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using SaleStore.Data;
using SaleStore.Models;
using System.Security.Claims;

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
    public Review? UserReview { get; set; }

    [BindProperty]
    public int Rating { get; set; } = 5;

    [BindProperty]
    public string? Comment { get; set; }

    public async Task<IActionResult> OnGetAsync(int id)
    {
        var product = await _context.Products.FirstOrDefaultAsync(p => p.Id == id && p.IsActive);
        if (product == null)
            return RedirectToAction("Index", "Home");

        Product = product;
        await LoadRelatedDataAsync(id);

        // Load existing review for the current user
        if (User.Identity?.IsAuthenticated == true)
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (long.TryParse(userIdClaim, out var userId))
            {
                UserReview = await _context.Reviews
                    .FirstOrDefaultAsync(r => r.ProductId == id && r.UserId == userId);
                if (UserReview != null)
                {
                    Rating = UserReview.Rating;
                    Comment = UserReview.Comment;
                }
            }
        }

        return Page();
    }

    public async Task<IActionResult> OnPostAsync(int id)
    {
        var product = await _context.Products.FirstOrDefaultAsync(p => p.Id == id && p.IsActive);
        if (product == null)
            return RedirectToAction("Index", "Home");

        Product = product;

        if (Rating < 1 || Rating > 5)
        {
            ModelState.AddModelError("", "Đánh giá phải từ 1 đến 5 sao.");
            await LoadRelatedDataAsync(id);
            return Page();
        }

        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!long.TryParse(userIdClaim, out var userId))
            return Forbid();

        var user = await _context.AppUsers.FirstOrDefaultAsync(u => u.Id == userId && u.IsActive);
        if (user == null)
            return Forbid();

        var existing = await _context.Reviews
            .FirstOrDefaultAsync(r => r.ProductId == id && r.UserId == userId);

        if (existing != null)
        {
            existing.Rating = Rating;
            existing.Comment = Comment?.Trim();
            existing.CreatedAt = DateTime.UtcNow;
        }
        else
        {
            _context.Reviews.Add(new Review
            {
                ProductId = id,
                UserId = userId,
                Rating = Rating,
                Comment = Comment?.Trim(),
                CreatedAt = DateTime.UtcNow
            });
        }

        await _context.SaveChangesAsync();
        TempData["ReviewSuccess"] = "Đã gửi đánh giá thành công!";
        return RedirectToPage(new { id });
    }

    private async Task LoadRelatedDataAsync(int productId)
    {
        RelatedProducts = await _context.Products
            .Where(p => p.IsActive && p.Category == Product.Category && p.Id != Product.Id)
            .OrderByDescending(p => p.Stock)
            .Take(4)
            .ToListAsync();

        Reviews = await _context.Reviews
            .Include(r => r.User)
            .Where(r => r.ProductId == productId)
            .OrderByDescending(r => r.CreatedAt)
            .Take(20)
            .ToListAsync();

        ReviewCount = Reviews.Count;
        AverageRating = ReviewCount > 0 ? Reviews.Average(r => r.Rating) : 0;
    }
}
