using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore; // Thêm dòng này để dùng các hàm Async của Database
using SaleStore.Data;
using SaleStore.Models;

namespace SaleStore.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = AppRoles.Admin)]
    public class DashboardController : Controller
    {
        // 1. Khai báo và "Tiêm" Database Context
        private readonly ApplicationDbContext _context;

        public DashboardController(ApplicationDbContext context)
        {
            _context = context;
        }

        // 2. Chuyển hàm thành Bất đồng bộ (async Task)
        public async Task<IActionResult> Index()
        {
            // Mẹo: Lấy thời điểm bắt đầu của ngày hôm nay (00:00:00) theo giờ chuẩn UTC
            var today = DateTime.UtcNow.Date;

            // Đếm số lượng trực tiếp từ Supabase
            ViewBag.TotalProducts    = await _context.Products.CountAsync();
            ViewBag.ActiveProducts   = await _context.Products.CountAsync(p => p.IsActive);
            ViewBag.TotalOrders      = await _context.Orders.CountAsync();
            ViewBag.PendingOrders    = await _context.Orders.CountAsync(o => o.Status == OrderStatus.Pending || o.Status == OrderStatus.Brewing);
            
            // Tính doanh thu hôm nay (Sửa o.Total thành o.TotalAmount)
            ViewBag.TodayRevenue     = await _context.Orders
                .Where(o => o.Status == OrderStatus.Delivered && o.CreatedAt >= today)
                .SumAsync(o => o.TotalAmount); 
                
            // Tính tổng doanh thu toàn thời gian
            ViewBag.TotalRevenue     = await _context.Orders
                .Where(o => o.Status == OrderStatus.Delivered)
                .SumAsync(o => o.TotalAmount);
                
            // Lấy 5 đơn hàng mới nhất (Kéo theo thông tin Khách hàng để hiển thị Tên)
            ViewBag.RecentOrders     = await _context.Orders
                .Include(o => o.Customer)
                .OrderByDescending(o => o.CreatedAt)
                .Take(5)
                .ToListAsync();

            return View();
        }
    }
}