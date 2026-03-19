using System;

namespace SaleStore.Models
{
    public class Customer
    {
        public long Id { get; set; }
        public string FullName { get; set; } = null!;
        public string Phone { get; set; } = null!;
        public DateTime CreatedAt { get; set; }
    }
}