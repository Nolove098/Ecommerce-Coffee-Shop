using Microsoft.AspNetCore.Mvc;
using SaleStore.Data;
using SaleStore.Models;

namespace SaleStore.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class OrderController : Controller
    {
        // GET: /Admin/Order
        public IActionResult Index(string? status)
        {
            var orders = MockDataStore.Orders.AsQueryable();

            if (!string.IsNullOrWhiteSpace(status) && Enum.TryParse<OrderStatus>(status, out var parsedStatus))
                orders = orders.Where(o => o.Status == parsedStatus);

            ViewBag.CurrentStatus = status;
            return View(orders.OrderByDescending(o => o.CreatedAt).ToList());
        }

        // GET: /Admin/Order/Detail/5
        public IActionResult Detail(long id)
        {
            var order = MockDataStore.Orders.FirstOrDefault(o => o.OrderID == id);
            if (order == null) return NotFound();
            return View(order);
        }

        // POST: /Admin/Order/UpdateStatus
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult UpdateStatus(long id, string status)
        {
            var order = MockDataStore.Orders.FirstOrDefault(o => o.OrderID == id);
            if (order == null) return NotFound();

            if (Enum.TryParse<OrderStatus>(status, out var newStatus))
            {
                order.Status = newStatus;
                TempData["Success"] = $"Đã cập nhật trạng thái đơn #{order.OrderID} thành \"{newStatus.ToVietnamese()}\".";
            }

            return RedirectToAction(nameof(Detail), new { id });
        }
    }
}
