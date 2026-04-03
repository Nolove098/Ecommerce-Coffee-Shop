using System;

namespace SaleStore.Models
{
    public class UserCartItem
    {
        public long Id { get; set; }
        public long UserId { get; set; }
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public int Quantity { get; set; }
        public string? ImageUrl { get; set; }
        public string? Category { get; set; }
        public DateTime UpdatedAt { get; set; }

        public AppUser? User { get; set; }
    }
}
