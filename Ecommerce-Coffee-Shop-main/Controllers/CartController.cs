using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using SaleStore.Data; // Đổi thành Ecommerce_Coffee_Shop.Data nếu bạn dùng namespace đó
using SaleStore.Models; // Đổi thành Ecommerce_Coffee_Shop.Models nếu bạn dùng namespace đó
using SaleStore.Models.ViewModels;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace SaleStore.Controllers; // Đổi thành Ecommerce_Coffee_Shop.Controllers nếu cần

public class CartController : Controller
{
    // 1. "Tiêm" ApplicationDbContext để gọi tới Supabase
    private readonly ApplicationDbContext _context;

    public CartController(ApplicationDbContext context)
    {
        _context = context;
    }

    // GET: /Cart/Checkout
    [Authorize]
    [HttpGet]
    [Route("Cart/Checkout")]
    public async Task<IActionResult> Checkout()
    {
        var model = new CheckoutViewModel();
        var email = User.FindFirstValue(ClaimTypes.Email);

        if (!string.IsNullOrWhiteSpace(email))
        {
            var user = await _context.AppUsers.FirstOrDefaultAsync(x => x.Email == email);
            if (user != null)
            {
                model.CustomerName = user.FullName;
                model.CustomerPhone = user.Phone ?? string.Empty;
            }
        }

        return View(model);
    }

    // POST: /Cart/PlaceOrder
    [Authorize]
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Route("Cart/PlaceOrder")]
    public async Task<IActionResult> PlaceOrder(CheckoutViewModel model)
    {
        if (!ModelState.IsValid)
            return View("Checkout", model);

        // Parse cart from JSON
        List<CartItem> cartItems;
        try
        {
            cartItems = JsonSerializer.Deserialize<List<CartItem>>(model.CartJson,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true })
                ?? new List<CartItem>();
        }
        catch (Exception ex)
        {
            ModelState.AddModelError("", "Lỗi xử lý giỏ hàng: " + ex.Message);
            return View("Checkout", model);
        }

        if (!cartItems.Any())
        {
            ModelState.AddModelError("", "Giỏ hàng của bạn đang trống. Vui lòng chọn sản phẩm.");
            return View("Checkout", model);
        }

        try
        {
            // 2. Xử lý thông tin Khách hàng (Tìm theo SĐT, nếu không có thì tạo mới)
            var customer = await _context.Customers.FirstOrDefaultAsync(c => c.Phone == model.CustomerPhone);
            if (customer == null)
            {
                customer = new Customer
                {
                    FullName = model.CustomerName,
                    Phone = model.CustomerPhone,
                    CreatedAt = DateTime.UtcNow
                };
                _context.Customers.Add(customer);
                await _context.SaveChangesAsync();
            }

            // 3. Xây dựng Đơn hàng
            var email = User.FindFirstValue(ClaimTypes.Email);
            var user = await _context.AppUsers.FirstOrDefaultAsync(x => x.Email == email);

            var order = new Order
            {
                CustomerId = customer.Id,
                CreatedByUserId = user?.Id, // Gắn ID user đang đăng nhập
                ShippingAddress = model.Address,
                Note = model.Note,
                Status = OrderStatus.Pending,
                TotalAmount = cartItems.Sum(c => c.Price * c.Quantity),
                CreatedAt = DateTime.UtcNow,
                
                OrderItems = cartItems.Select(c => new OrderItem
                {
                    ProductId = c.ProductId,
                    ProductName = c.ProductName,
                    UnitPrice = c.Price,
                    Quantity = c.Quantity
                }).ToList()
            };

            _context.Orders.Add(order);
            await _context.SaveChangesAsync();

            return RedirectToAction("Success", new { id = order.Id });
        }
        catch (Exception ex)
        {
            // Log error here in a real app
            ViewBag.Error = "Đã xảy ra lỗi khi lưu đơn hàng: " + (ex.InnerException?.Message ?? ex.Message);
            return View("Checkout", model);
        }
    }

    // GET: /Cart/Success/{id}
    [HttpGet]
    [Route("Cart/Success/{id:long}")]
    public async Task<IActionResult> Success(long id)
    {
        // 5. Lấy đơn hàng từ Supabase (Kéo theo cả thông tin Customer và OrderItems)
        var order = await _context.Orders
            .Include(o => o.Customer)
            .Include(o => o.OrderItems)
            .FirstOrDefaultAsync(o => o.Id == id);

        if (order == null) return RedirectToAction("Index", "Home");
        
        return View(order);
    }
}