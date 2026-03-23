using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Http;
using System.Text.Json;
using SaleStore.Models;
using SaleStore.Data;

namespace SaleStore.Pages
{
    public class CheckoutModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public CheckoutModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public List<CartItem> CartItems { get; set; } = new List<CartItem>();
        public decimal TotalAmount { get; set; }
        public decimal ShippingFee { get; set; } = 30000; // 30,000 VND
        public decimal GrandTotal { get; set; }

        [BindProperty]
        public CheckoutInfo CheckoutInfo { get; set; } = new CheckoutInfo();

        public void OnGet()
        {
            LoadCart();
        }

        public IActionResult OnPost()
        {
            if (!ModelState.IsValid)
            {
                LoadCart();
                return Page();
            }

            // Process order
            try
            {
                var cartJson = HttpContext.Session.GetString("Cart");
                if (string.IsNullOrEmpty(cartJson))
                {
                    TempData["Error"] = "Giỏ hàng trống";
                    return RedirectToPage("/Menu");
                }

                var cartItems = JsonSerializer.Deserialize<List<CartItem>>(cartJson);
                if (cartItems == null || !cartItems.Any())
                {
                    TempData["Error"] = "Giỏ hàng trống";
                    return RedirectToPage("/Menu");
                }

                // Create or get customer
                var customer = _context.Customers
                    .FirstOrDefault(c => c.Phone == CheckoutInfo.Phone);

                if (customer == null)
                {
                    customer = new Customer
                    {
                        FullName = CheckoutInfo.FullName,
                        Phone = CheckoutInfo.Phone,
                        CreatedAt = DateTime.Now
                    };
                    _context.Customers.Add(customer);
                    _context.SaveChanges();
                }

                // Create order
                var order = new Order
                {
                    CustomerId = customer.Id,
                    ShippingAddress = CheckoutInfo.Address,
                    Note = CheckoutInfo.Note,
                    Status = OrderStatus.Pending,
                    TotalAmount = cartItems.Sum(item => item.Price * item.Quantity) + ShippingFee,
                    CreatedAt = DateTime.Now,
                    OrderItems = cartItems.Select(item => new OrderItem
                    {
                        ProductId = item.ProductId,
                        ProductName = item.ProductName,
                        Quantity = item.Quantity,
                        UnitPrice = item.Price,
                        LineTotal = item.Price * item.Quantity
                    }).ToList()
                };

                _context.Orders.Add(order);
                _context.SaveChanges();

                // Clear cart
                HttpContext.Session.Remove("Cart");

                TempData["Success"] = "Đặt hàng thành công!";
                return RedirectToPage("/OrderSuccess", new { orderId = order.Id });
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Có lỗi xảy ra: " + ex.Message;
                LoadCart();
                return Page();
            }
        }

        private void LoadCart()
        {
            var cartJson = HttpContext.Session.GetString("Cart");
            if (!string.IsNullOrEmpty(cartJson))
            {
                CartItems = JsonSerializer.Deserialize<List<CartItem>>(cartJson) ?? new List<CartItem>();
                TotalAmount = CartItems.Sum(item => item.Price * item.Quantity);
                GrandTotal = TotalAmount + ShippingFee;
            }
        }
    }

    public class CheckoutInfo
    {
        public string FullName { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string Note { get; set; } = string.Empty;
        public string PaymentMethod { get; set; } = "COD";
    }
}
