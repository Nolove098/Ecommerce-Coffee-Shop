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
            // Lấy thời điểm bắt đầu ngày hôm nay theo giờ Việt Nam (UTC+7)
            var vnZone = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");
            var today = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, vnZone).Date;
            // Chuyển ngưỡng ngày VN về UTC để query DB
            var todayUtc = TimeZoneInfo.ConvertTimeToUtc(today, vnZone);

            // Đếm số lượng trực tiếp từ Supabase
            ViewBag.TotalProducts    = await _context.Products.CountAsync();
            ViewBag.ActiveProducts   = await _context.Products.CountAsync(p => p.IsActive);
            ViewBag.TotalOrders      = await _context.Orders.CountAsync();
            ViewBag.PendingOrders    = await _context.Orders.CountAsync(o => o.Status == OrderStatus.Pending);
            
            // Tính doanh thu hôm nay (Sửa o.Total thành o.TotalAmount)
            ViewBag.TodayRevenue     = await _context.Orders
                .Where(o => o.Status == OrderStatus.Delivered && o.CreatedAt >= todayUtc)
                .SumAsync(o => o.TotalAmount); 
                
            // Tính tổng doanh thu toàn thời gian
            ViewBag.TotalRevenue     = await _context.Orders
                .Where(o => o.Status == OrderStatus.Delivered)
                .SumAsync(o => o.TotalAmount);

            return View();
        }
    }
}