using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using System.Text.Json;
using SaleStore.Models;

namespace SaleStore.ViewComponents
{
    public class CartSidebarViewComponent : ViewComponent
    {
        public IViewComponentResult Invoke()
        {
            var cartJson = HttpContext.Session.GetString("Cart");
            var cartItems = new List<CartItem>();
            
            if (!string.IsNullOrEmpty(cartJson))
            {
                try
                {
                    var items = JsonSerializer.Deserialize<List<CartItem>>(cartJson);
                    if (items != null)
                    {
                        // Filter out any invalid items with comprehensive null checks
                        cartItems = items.Where(item => 
                            item != null && 
                            item.ProductId > 0 && 
                            item.Price > 0 && 
                            item.Quantity > 0 &&
                            !string.IsNullOrEmpty(item.ProductName)
                        ).ToList();
                    }
                }
                catch (Exception)
                {
                    // If deserialization fails, clear the cart and start fresh
                    HttpContext.Session.Remove("Cart");
                    cartItems = new List<CartItem>();
                }
            }

            // Calculate totals with safe null handling
            decimal totalAmount = 0;
            int itemCount = 0;
            
            try
            {
                totalAmount = cartItems.Any() ? cartItems.Sum(item => item.Price * item.Quantity) : 0;
                itemCount = cartItems.Any() ? cartItems.Sum(item => item.Quantity) : 0;
            }
            catch
            {
                // If calculation fails, use defaults
                totalAmount = 0;
                itemCount = 0;
            }

            var viewModel = new CartSidebarViewModel
            {
                CartItems = cartItems ?? new List<CartItem>(),
                TotalAmount = totalAmount,
                ItemCount = itemCount
            };

            return View(viewModel);
        }
    }

    public class CartSidebarViewModel
    {
        public List<CartItem> CartItems { get; set; } = new List<CartItem>();
        public decimal TotalAmount { get; set; }
        public int ItemCount { get; set; }
    }
}
