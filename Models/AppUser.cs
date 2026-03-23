using System;
using System.Collections.Generic;

namespace SaleStore.Models
{
    public class AppUser
    {
        public long Id { get; set; }
        public string Username { get; set; } = null!;
        public string FullName { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string? Phone { get; set; }
        public string PasswordHash { get; set; } = null!;
        public string PasswordSalt { get; set; } = null!;
        public string Role { get; set; } = AppRoles.User;
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public DateTime? LastLoginAt { get; set; }

        public ICollection<AuthActivity> AuthActivities { get; set; } = new List<AuthActivity>();
    }
}