using Microsoft.AspNetCore.Mvc;
using SaleStore.Data;
using SaleStore.Models;

namespace SaleStore.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class ProductController : Controller
    {
        // GET: /Admin/Product
        public IActionResult Index(string? search, string? category)
        {
            var products = MockDataStore.Products.AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
                products = products.Where(p => p.Name.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                                               p.Category.Contains(search, StringComparison.OrdinalIgnoreCase));

            if (!string.IsNullOrWhiteSpace(category))
                products = products.Where(p => p.Category == category);

            ViewBag.Search     = search;
            ViewBag.Category   = category;
            ViewBag.Categories = MockDataStore.Products.Select(p => p.Category).Distinct().OrderBy(c => c).ToList();

            return View(products.OrderBy(p => p.ProductID).ToList());
        }

        // GET: /Admin/Product/Create
        public IActionResult Create()
        {
            ViewBag.Categories = MockDataStore.Products.Select(p => p.Category).Distinct().OrderBy(c => c).ToList();
            return View(new Product());
        }

        // POST: /Admin/Product/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Product product)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Categories = MockDataStore.Products.Select(p => p.Category).Distinct().OrderBy(c => c).ToList();
                return View(product);
            }

            product.ProductID = MockDataStore.NextProductId();
            MockDataStore.Products.Add(product);
            TempData["Success"] = $"Đã thêm sản phẩm \"{product.Name}\" thành công!";
            return RedirectToAction(nameof(Index));
        }

        // GET: /Admin/Product/Edit/5
        public IActionResult Edit(long id)
        {
            var product = MockDataStore.Products.FirstOrDefault(p => p.ProductID == id);
            if (product == null) return NotFound();

            ViewBag.Categories = MockDataStore.Products.Select(p => p.Category).Distinct().OrderBy(c => c).ToList();
            return View(product);
        }

        // POST: /Admin/Product/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(long id, Product product)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Categories = MockDataStore.Products.Select(p => p.Category).Distinct().OrderBy(c => c).ToList();
                return View(product);
            }

            var existing = MockDataStore.Products.FirstOrDefault(p => p.ProductID == id);
            if (existing == null) return NotFound();

            existing.Name        = product.Name;
            existing.Description = product.Description;
            existing.Price       = product.Price;
            existing.Category    = product.Category;
            existing.ImageUrl    = product.ImageUrl;
            existing.Stock       = product.Stock;
            existing.IsActive    = product.IsActive;

            TempData["Success"] = $"Đã cập nhật sản phẩm \"{existing.Name}\" thành công!";
            return RedirectToAction(nameof(Index));
        }

        // GET: /Admin/Product/Delete/5
        public IActionResult Delete(long id)
        {
            var product = MockDataStore.Products.FirstOrDefault(p => p.ProductID == id);
            if (product == null) return NotFound();
            return View(product);
        }

        // POST: /Admin/Product/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(long id)
        {
            var product = MockDataStore.Products.FirstOrDefault(p => p.ProductID == id);
            if (product != null)
            {
                MockDataStore.Products.Remove(product);
                TempData["Success"] = $"Đã xóa sản phẩm \"{product.Name}\".";
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
