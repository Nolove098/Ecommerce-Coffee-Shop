namespace SaleStore.Models
{
    public enum OrderStatus
    {
        Pending = 0,    // Chờ xử lý
        Ready = 2,      // Sẵn sàng
        Delivered = 3,  // Đã giao
        Cancelled = 4   // Đã hủy
    }

    public static class OrderStatusExtensions
    {
        public static string ToVietnamese(this OrderStatus status) => status switch
        {
            OrderStatus.Pending   => "Chờ xử lý",
            OrderStatus.Ready     => "Sẵn sàng",
            OrderStatus.Delivered => "Đã giao",
            OrderStatus.Cancelled => "Đã hủy",
            _                     => "Chờ xử lý"
        };

        public static string ToBadgeClass(this OrderStatus status) => status switch
        {
            OrderStatus.Pending   => "badge-pending",
            OrderStatus.Ready     => "badge-ready",
            OrderStatus.Delivered => "badge-delivered",
            OrderStatus.Cancelled => "badge-cancelled",
            _                     => "badge-pending"
        };
    }
}
