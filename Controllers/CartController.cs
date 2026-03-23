using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using SaleStore.Models;

namespace SaleStore.Controllers
{
    public class CartController : Controller
    {
        [HttpPost]
        public IActionResult AddToCart([FromBody] CartItem item)
        {
            try
            {
                // Validate input
                if (item == null || item.ProductId <= 0 || item.Price <= 0 || item.Quantity <= 0)
                {
                    return Json(new { success = false, message = "Invalid cart item data" });
                }

                var cartJson = HttpContext.Session.GetString("Cart");
                var cart = string.IsNullOrEmpty(cartJson) 
                    ? new List<CartItem>() 
                    : JsonSerializer.Deserialize<List<CartItem>>(cartJson) ?? new List<CartItem>();

                var existingItem = cart.FirstOrDefault(c => c.ProductId == item.ProductId);
                if (existingItem != null)
                {
                    existingItem.Quantity += item.Quantity;
                }
                else
                {
                    cart.Add(item);
                }

                HttpContext.Session.SetString("Cart", JsonSerializer.Serialize(cart));

                return Json(new { success = true, itemCount = cart.Sum(c => c.Quantity) });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        public IActionResult UpdateQuantity([FromBody] UpdateQuantityRequest request)
        {
            var cartJson = HttpContext.Session.GetString("Cart");
            if (string.IsNullOrEmpty(cartJson))
            {
                return Json(new { success = false, message = "Cart is empty" });
            }

            var cart = JsonSerializer.Deserialize<List<CartItem>>(cartJson) ?? new List<CartItem>();
            var item = cart.FirstOrDefault(c => c.ProductId == request.ProductId);

            if (item != null)
            {
                item.Quantity += request.Change;
                if (item.Quantity <= 0)
                {
                    cart.Remove(item);
                }

                HttpContext.Session.SetString("Cart", JsonSerializer.Serialize(cart));
                return Json(new { success = true, itemCount = cart.Sum(c => c.Quantity), totalAmount = cart.Sum(c => c.Price * c.Quantity) });
            }

            return Json(new { success = false, message = "Item not found" });
        }

        [HttpPost]
        public IActionResult RemoveFromCart([FromBody] RemoveItemRequest request)
        {
            var cartJson = HttpContext.Session.GetString("Cart");
            if (string.IsNullOrEmpty(cartJson))
            {
                return Json(new { success = false, message = "Cart is empty" });
            }

            var cart = JsonSerializer.Deserialize<List<CartItem>>(cartJson) ?? new List<CartItem>();
            var item = cart.FirstOrDefault(c => c.ProductId == request.ProductId);

            if (item != null)
            {
                cart.Remove(item);
                HttpContext.Session.SetString("Cart", JsonSerializer.Serialize(cart));
                return Json(new { success = true, itemCount = cart.Sum(c => c.Quantity), totalAmount = cart.Sum(c => c.Price * c.Quantity) });
            }

            return Json(new { success = false, message = "Item not found" });
        }

        [HttpGet]
        public IActionResult GetCart()
        {
            var cartJson = HttpContext.Session.GetString("Cart");
            var cart = string.IsNullOrEmpty(cartJson) 
                ? new List<CartItem>() 
                : JsonSerializer.Deserialize<List<CartItem>>(cartJson) ?? new List<CartItem>();

            return Json(new 
            { 
                items = cart, 
                itemCount = cart.Sum(c => c.Quantity),
                totalAmount = cart.Sum(c => c.Price * c.Quantity)
            });
        }

        [HttpPost]
        public IActionResult ClearCart()
        {
            HttpContext.Session.Remove("Cart");
            return Json(new { success = true });
        }
    }

    public class UpdateQuantityRequest
    {
        public long ProductId { get; set; }
        public int Change { get; set; }
    }

    public class RemoveItemRequest
    {
        public long ProductId { get; set; }
    }
}
