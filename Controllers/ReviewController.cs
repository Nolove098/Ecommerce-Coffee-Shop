using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SaleStore.Data;
using SaleStore.Models;
using System.Security.Claims;

namespace SaleStore.Controllers
{
    public class ReviewController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ReviewController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetByProduct(long productId)
        {
            var reviews = await _context.Reviews
                .Include(r => r.User)
                .Where(r => r.ProductId == productId)
                .OrderByDescending(r => r.CreatedAt)
                .Select(r => new
                {
                    r.Rating,
                    r.Comment,
                    UserName = r.User != null ? r.User.FullName : "Ẩn danh",
                    CreatedAt = r.CreatedAt.AddHours(7).ToString("dd/MM/yyyy")
                })
                .ToListAsync();

            return Json(reviews);
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(long productId, int rating, string? comment)
        {
            if (rating < 1 || rating > 5)
                return BadRequest("Đánh giá phải từ 1 đến 5 sao.");

            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!long.TryParse(userIdClaim, out var userId))
                return Unauthorized();

            var user = await _context.AppUsers.FirstOrDefaultAsync(u => u.Id == userId && u.IsActive);
            if (user == null)
                return Unauthorized();

            var product = await _context.Products.FindAsync(productId);
            if (product == null)
                return NotFound("Sản phẩm không tồn tại.");

            // Always create a new review (allow multiple reviews per user)
            _context.Reviews.Add(new Review
            {
                ProductId = productId,
                UserId = user.Id,
                Rating = rating,
                Comment = comment?.Trim(),
                CreatedAt = DateTime.UtcNow
            });

            await _context.SaveChangesAsync();

            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                return Json(new { success = true, message = "Đã gửi đánh giá thành công!" });

            TempData["Success"] = "Đã gửi đánh giá thành công!";
            return Redirect($"/san-pham/{productId}");
        }
    }
}
