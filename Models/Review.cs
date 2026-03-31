using System;

namespace SaleStore.Models
{
    public class Review
    {
        public long Id { get; set; }
        public long ProductId { get; set; }
        public long UserId { get; set; }
        public int Rating { get; set; }
        public string? Comment { get; set; }
        public DateTime CreatedAt { get; set; }

        public Product? Product { get; set; }
        public AppUser? User { get; set; }
    }
}
