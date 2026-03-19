using System;
using System.Collections.Generic;

namespace SaleStore.Models
{
    public class Order
    {
        public long Id { get; set; }
        public long CustomerId { get; set; }
        public string? ShippingAddress { get; set; }
        public string? Note { get; set; }
        public OrderStatus Status { get; set; }
        public decimal TotalAmount { get; set; }
        public DateTime CreatedAt { get; set; }

        // Khai báo mối quan hệ (Navigation properties) giúp nối bảng dễ dàng hơn
        public Customer? Customer { get; set; }
        public List<OrderItem> OrderItems { get; set; } = new();
    }
}