using System.ComponentModel.DataAnnotations;

namespace SaleStore.Models
{
    public class Order
    {
        public long OrderID { get; set; }

        [Display(Name = "Khách hàng")]
        public string CustomerName { get; set; } = string.Empty;

        [Display(Name = "Số điện thoại")]
        public string CustomerPhone { get; set; } = string.Empty;

        [Display(Name = "Ghi chú")]
        public string Note { get; set; } = string.Empty;

        public List<OrderItem> Items { get; set; } = new();

        [Display(Name = "Trạng thái")]
        public OrderStatus Status { get; set; } = OrderStatus.Pending;

        [Display(Name = "Thời gian đặt")]
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public decimal Total => Items.Sum(i => i.Subtotal);
    }
}
