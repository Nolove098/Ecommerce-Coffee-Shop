using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SaleStore.Data;
using SaleStore.Models;
using SaleStore.Models.ViewModels;
using System.Security.Claims;

namespace SaleStore.Areas.Staff.Controllers
{
    [Area("Staff")]
    [Authorize(Roles = AppRoles.Staff + "," + AppRoles.Admin)]
    public class POSController : Controller
    {
        private const string WalkInPhone = "POS-WALKIN";
        private readonly ApplicationDbContext _context;

        public POSController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var products = await _context.Products
                .AsNoTracking()
                .Where(x => x.IsActive)
                .OrderBy(x => x.Category)
                .ThenBy(x => x.Name)
                .Select(x => new PosProductViewModel
                {
                    Id = x.Id,
                    Name = x.Name,
                    Description = x.Description,
                    Price = x.Price,
                    Category = x.Category,
                    ImageUrl = x.ImageUrl,
                    Stock = x.Stock
                })
                .ToListAsync();

            var model = new PosIndexViewModel
            {
                OperatorName = User.Identity?.Name ?? "Nhân viên",
                CanAccessAdmin = User.IsInRole(AppRoles.Admin),
                Categories = products.Select(x => x.Category).Distinct().OrderBy(x => x).ToList(),
                Products = products
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateOrder([FromBody] PosCheckoutRequest request)
        {
            if (request.Items == null || request.Items.Count == 0)
                return BadRequest(new { message = "Chưa có món nào trong hóa đơn." });

            var operatorUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!long.TryParse(operatorUserId, out var createdByUserId))
                return Unauthorized(new { message = "Không xác định được nhân viên đang đăng nhập." });

            var validItems = request.Items
                .Where(x => x.ProductId > 0 && x.Quantity > 0)
                .GroupBy(x => x.ProductId)
                .Select(x => new PosCartItemInputModel
                {
                    ProductId = x.Key,
                    Quantity = x.Sum(y => y.Quantity),
                    Price = x.Last().Price
                })
                .ToList();

            if (validItems.Count == 0)
                return BadRequest(new { message = "Danh sách món không hợp lệ." });

            var productIds = validItems.Select(x => x.ProductId).ToList();
            var products = await _context.Products
                .Where(x => x.IsActive && productIds.Contains(x.Id))
                .ToDictionaryAsync(x => x.Id);

            if (products.Count != productIds.Count)
                return BadRequest(new { message = "Một số sản phẩm không còn khả dụng." });

            var items = new List<OrderItem>();
            foreach (var item in validItems)
            {
                var product = products[item.ProductId];

                if (product.Stock <= 0)
                    return BadRequest(new { message = $"{product.Name} hiện đã hết hàng." });

                if (product.Stock < item.Quantity)
                    return BadRequest(new { message = $"{product.Name} chỉ còn {product.Stock} sản phẩm trong kho." });

                items.Add(new OrderItem
                {
                    ProductId = product.Id,
                    ProductName = product.Name,
                    UnitPrice = product.Price,
                    Quantity = item.Quantity
                });
            }

            var customer = await ResolveCustomerAsync(request.CustomerName, request.CustomerPhone);
            var orderCustomerName = string.IsNullOrWhiteSpace(request.CustomerName)
                ? customer.FullName
                : request.CustomerName.Trim();

            var order = new Order
            {
                CustomerId = customer.Id,
                CustomerName = orderCustomerName,
                CreatedByUserId = createdByUserId,
                ShippingAddress = "Mua tại quầy",
                Note = string.IsNullOrWhiteSpace(request.Note) ? "Đơn POS tại quầy" : request.Note.Trim(),
                Status = OrderStatus.Pending,
                TotalAmount = items.Sum(x => x.UnitPrice * x.Quantity),
                CreatedAt = DateTime.UtcNow,
                OrderItems = items
            };

            _context.Orders.Add(order);
            await _context.SaveChangesAsync();

            return Json(new
            {
                success = true,
                message = "Thanh toán thành công",
                orderId = order.Id,
                totalAmount = order.TotalAmount,
                status = order.Status.ToString(),
                statusText = order.Status.ToVietnamese(),
                createdAt = order.CreatedAt.ToLocalTime().ToString("HH:mm dd/MM/yyyy"),
                operatorName = User.FindFirstValue(ClaimTypes.Name) ?? User.Identity?.Name ?? "Nhân viên"
            });
        }

        private async Task<Customer> ResolveCustomerAsync(string? customerName, string? customerPhone)
        {
            var normalizedName = string.IsNullOrWhiteSpace(customerName) ? null : customerName.Trim();
            var normalizedPhone = string.IsNullOrWhiteSpace(customerPhone) ? null : customerPhone.Trim();

            if (!string.IsNullOrWhiteSpace(normalizedPhone))
            {
                var existingCustomer = await _context.Customers.FirstOrDefaultAsync(x => x.Phone == normalizedPhone);
                if (existingCustomer != null)
                {
                    if (!string.IsNullOrWhiteSpace(normalizedName))
                        existingCustomer.FullName = normalizedName;

                    return existingCustomer;
                }

                var phoneCustomer = new Customer
                {
                    FullName = normalizedName ?? "Khách tại quầy",
                    Phone = normalizedPhone,
                    CreatedAt = DateTime.UtcNow
                };

                _context.Customers.Add(phoneCustomer);
                await _context.SaveChangesAsync();
                return phoneCustomer;
            }

            if (!string.IsNullOrWhiteSpace(normalizedName))
            {
                var namedCustomer = new Customer
                {
                    FullName = normalizedName,
                    Phone = $"POS-{DateTime.UtcNow:yyyyMMddHHmmssfff}",
                    CreatedAt = DateTime.UtcNow
                };

                _context.Customers.Add(namedCustomer);
                await _context.SaveChangesAsync();
                return namedCustomer;
            }

            var walkInCustomer = await _context.Customers.FirstOrDefaultAsync(x => x.Phone == WalkInPhone);
            if (walkInCustomer != null)
                return walkInCustomer;

            walkInCustomer = new Customer
            {
                FullName = "Khách tại quầy",
                Phone = WalkInPhone,
                CreatedAt = DateTime.UtcNow
            };

            _context.Customers.Add(walkInCustomer);
            await _context.SaveChangesAsync();
            return walkInCustomer;
        }
    }
}