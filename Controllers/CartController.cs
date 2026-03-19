using Microsoft.AspNetCore.Mvc;
using SaleStore.Data; // Đổi thành Ecommerce_Coffee_Shop.Data nếu bạn dùng namespace đó
using SaleStore.Models; // Đổi thành Ecommerce_Coffee_Shop.Models nếu bạn dùng namespace đó
using SaleStore.Models.ViewModels;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;

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
    public IActionResult Checkout()
    {
        return View(new CheckoutViewModel());
    }

    // POST: /Cart/PlaceOrder
    [HttpPost]
    [ValidateAntiForgeryToken]
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
        catch
        {
            cartItems = new List<CartItem>();
        }

        if (!cartItems.Any())
        {
            ModelState.AddModelError("", "Giỏ hàng của bạn đang trống. Vui lòng chọn sản phẩm.");
            return View("Checkout", model);
        }

        // 2. Xử lý thông tin Khách hàng (Tìm theo SĐT, nếu không có thì tạo mới)
        var customer = await _context.Customers.FirstOrDefaultAsync(c => c.Phone == model.CustomerPhone);
        if (customer == null)
        {
            customer = new Customer
            {
                FullName = model.CustomerName,
                Phone = model.CustomerPhone,
                CreatedAt = DateTime.UtcNow // Dùng giờ UTC cho chuẩn quốc tế của Postgres
            };
            _context.Customers.Add(customer);
            await _context.SaveChangesAsync(); // Lưu để lấy được Customer.Id
        }

        // 3. Xây dựng Đơn hàng (Lưu vào bảng orders)
        var order = new Order
        {
            CustomerId = customer.Id, // Gắn ID khách hàng vào đơn
            ShippingAddress = model.Address,
            Note = model.Note,
            Status = OrderStatus.Pending,
            TotalAmount = cartItems.Sum(c => c.Price * c.Quantity), // Tính tổng tiền
            CreatedAt = DateTime.UtcNow,
            
            // Xây dựng Chi tiết đơn hàng (Lưu vào bảng order_items)
            OrderItems = cartItems.Select(c => new OrderItem
            {
                ProductId = c.ProductId,
                ProductName = c.ProductName,
                UnitPrice = c.Price,
                Quantity = c.Quantity
            }).ToList()
        };

        // 4. Lưu đơn hàng và chi tiết vào Supabase
        _context.Orders.Add(order);
        await _context.SaveChangesAsync(); // Hàm này sẽ tự động lưu cả Order và OrderItems

        // Chuyển hướng sang trang thành công với ID mới tạo
        return RedirectToAction("Success", new { id = order.Id });
    }

    // GET: /Cart/Success/{id}
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