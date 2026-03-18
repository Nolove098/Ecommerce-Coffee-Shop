using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore; // Thêm thư viện này để dùng các hàm Async của Database
using SaleStore.Data;
using SaleStore.Models;

namespace SaleStore.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class ProductController : Controller
    {
        // 1. Gọi Database Context
        private readonly ApplicationDbContext _context;

        public ProductController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: /Admin/Product
        // Chuyển thành async Task<IActionResult>
        public async Task<IActionResult> Index(string? search, string? category)
        {
            // Lấy thẳng từ bảng Products trong Supabase
            var products = _context.Products.AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                // Note: EF Core sẽ tự dịch đoạn này thành câu lệnh SQL LIKE
                products = products.Where(p => EF.Functions.ILike(p.Name, $"%{search}%") || 
                                               EF.Functions.ILike(p.Category, $"%{search}%"));
            }

            if (!string.IsNullOrWhiteSpace(category))
            {
                products = products.Where(p => p.Category == category);
            }

            ViewBag.Search     = search;
            ViewBag.Category   = category;
            
            // Lấy danh sách danh mục (để làm bộ lọc)
            ViewBag.Categories = await _context.Products
                                       .Select(p => p.Category)
                                       .Distinct()
                                       .OrderBy(c => c)
                                       .ToListAsync();

            // Sửa ProductID thành Id
            return View(await products.OrderBy(p => p.Id).ToListAsync());
        }

        // GET: /Admin/Product/Create
        public async Task<IActionResult> Create()
        {
            ViewBag.Categories = await _context.Products
                                       .Select(p => p.Category)
                                       .Distinct()
                                       .OrderBy(c => c)
                                       .ToListAsync();
            return View(new Product());
        }

        // POST: /Admin/Product/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Product product)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Categories = await _context.Products.Select(p => p.Category).Distinct().OrderBy(c => c).ToListAsync();
                return View(product);
            }

            // KHÔNG CẦN TẠO ID NỮA, Supabase sẽ tự lo việc đó!
            product.CreatedAt = DateTime.UtcNow;
            product.UpdatedAt = DateTime.UtcNow;

            _context.Products.Add(product);
            await _context.SaveChangesAsync(); // Lưu vào Database

            TempData["Success"] = $"Đã thêm sản phẩm \"{product.Name}\" thành công!";
            return RedirectToAction(nameof(Index));
        }

        // GET: /Admin/Product/Edit/5
        public async Task<IActionResult> Edit(long id)
        {
            // Sửa ProductID thành Id
            var product = await _context.Products.FirstOrDefaultAsync(p => p.Id == id);
            if (product == null) return NotFound();

            ViewBag.Categories = await _context.Products.Select(p => p.Category).Distinct().OrderBy(c => c).ToListAsync();
            return View(product);
        }

        // POST: /Admin/Product/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(long id, Product product)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Categories = await _context.Products.Select(p => p.Category).Distinct().OrderBy(c => c).ToListAsync();
                return View(product);
            }

            // Sửa ProductID thành Id
            var existing = await _context.Products.FirstOrDefaultAsync(p => p.Id == id);
            if (existing == null) return NotFound();

            // Cập nhật các trường
            existing.Name        = product.Name;
            existing.Description = product.Description;
            existing.Price       = product.Price;
            existing.Category    = product.Category;
            existing.ImageUrl    = product.ImageUrl;
            existing.Stock       = product.Stock;
            existing.IsActive    = product.IsActive;
            existing.UpdatedAt   = DateTime.UtcNow; // Cập nhật thời gian sửa

            await _context.SaveChangesAsync(); // Lưu thay đổi vào DB

            TempData["Success"] = $"Đã cập nhật sản phẩm \"{existing.Name}\" thành công!";
            return RedirectToAction(nameof(Index));
        }

        // GET: /Admin/Product/Delete/5
        public async Task<IActionResult> Delete(long id)
        {
            var product = await _context.Products.FirstOrDefaultAsync(p => p.Id == id);
            if (product == null) return NotFound();
            return View(product);
        }

        // POST: /Admin/Product/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(long id)
        {
            var product = await _context.Products.FirstOrDefaultAsync(p => p.Id == id);
            if (product != null)
            {
                _context.Products.Remove(product);
                await _context.SaveChangesAsync(); // Xóa khỏi DB thật
                
                TempData["Success"] = $"Đã xóa sản phẩm \"{product.Name}\".";
            }
            return RedirectToAction(nameof(Index));
        }
    }
}