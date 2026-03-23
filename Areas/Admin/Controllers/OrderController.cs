using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore; // Bắt buộc phải có để dùng Async và Include()
using SaleStore.Data;
using SaleStore.Models;

namespace SaleStore.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = AppRoles.Admin)]
    public class OrderController : Controller
    {
        // 1. Gọi Database Context
        private readonly ApplicationDbContext _context;

        public OrderController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: /Admin/Order
        public async Task<IActionResult> Index(string? status)
        {
            // Lấy danh sách đơn hàng từ DB, "Include" thêm thông tin Khách hàng (Customer)
            var orders = _context.Orders.Include(o => o.Customer).AsQueryable();

            if (!string.IsNullOrWhiteSpace(status) && Enum.TryParse<OrderStatus>(status, out var parsedStatus))
                orders = orders.Where(o => o.Status == parsedStatus);

            ViewBag.CurrentStatus = status;
            
            // Trả về danh sách đã sắp xếp mới nhất lên đầu
            return View(await orders.OrderByDescending(o => o.CreatedAt).ToListAsync());
        }

        // GET: /Admin/Order/Detail/5
        public async Task<IActionResult> Detail(long id)
        {
            // Lấy chi tiết đơn hàng (Đổi OrderID thành Id)
            // Kéo theo cả thông tin Khách hàng (Customer) và Danh sách món nước (OrderItems)
            var order = await _context.Orders
                                      .Include(o => o.Customer)
                                      .Include(o => o.OrderItems)
                                      .FirstOrDefaultAsync(o => o.Id == id);
                                      
            if (order == null) return NotFound();
            
            return View(order);
        }

        // POST: /Admin/Order/UpdateStatus
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateStatus(long id, string status)
        {
            // Tìm đơn hàng cần cập nhật (Đổi OrderID thành Id)
            var order = await _context.Orders.FirstOrDefaultAsync(o => o.Id == id);
            if (order == null) return NotFound();

            if (Enum.TryParse<OrderStatus>(status, out var newStatus))
            {
                // Cập nhật trạng thái
                order.Status = newStatus;
                
                // Lưu thay đổi vào Supabase
                await _context.SaveChangesAsync();
                
                TempData["Success"] = $"Đã cập nhật trạng thái đơn #{order.Id} thành \"{newStatus.ToVietnamese()}\".";
            }

            return RedirectToAction(nameof(Detail), new { id });
        }
    }
}