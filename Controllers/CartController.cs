using Microsoft.AspNetCore.Mvc;
using SaleStore.Data;
using SaleStore.Models;
using SaleStore.Models.ViewModels;
using System.Text.Json;

namespace SaleStore.Controllers;

public class CartController : Controller
{
    // GET: /Cart/Checkout
    public IActionResult Checkout()
    {
        return View(new CheckoutViewModel());
    }

    // POST: /Cart/PlaceOrder
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult PlaceOrder(CheckoutViewModel model)
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

        // Build order
        var order = new Order
        {
            OrderID    = MockDataStore.NextOrderId(),
            CustomerName  = model.CustomerName,
            CustomerPhone = model.CustomerPhone,
            Note       = string.IsNullOrWhiteSpace(model.Note)
                            ? (model.Address ?? "")
                            : $"{model.Address} – {model.Note}",
            Status     = OrderStatus.Pending,
            CreatedAt  = DateTime.Now,
            Items      = cartItems.Select(c => new OrderItem
            {
                ProductID   = (long)c.ProductId,
                ProductName = c.ProductName,
                UnitPrice   = c.Price,
                Quantity    = c.Quantity,
            }).ToList(),
        };

        MockDataStore.Orders.Add(order);

        return RedirectToAction("Success", new { id = order.OrderID });
    }

    // GET: /Cart/Success/{id}
    public IActionResult Success(int id)
    {
        var order = MockDataStore.Orders.FirstOrDefault(o => o.OrderID == id);
        if (order == null) return RedirectToAction("Index", "Home");
        return View(order);
    }
}
