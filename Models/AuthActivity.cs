using System;

namespace SaleStore.Models
{
    public class AuthActivity
    {
        public long Id { get; set; }
        public long UserId { get; set; }
        public string ActivityType { get; set; } = null!;
        public string? IpAddress { get; set; }
        public string? UserAgent { get; set; }
        public DateTime CreatedAt { get; set; }

        public AppUser User { get; set; } = null!;
    }
}