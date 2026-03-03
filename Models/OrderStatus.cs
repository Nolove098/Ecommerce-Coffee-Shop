namespace SaleStore.Models
{
    public enum OrderStatus
    {
        Pending,    // Chờ xử lý
        Brewing,    // Đang pha chế
        Ready,      // Sẵn sàng
        Delivered,  // Đã giao
        Cancelled   // Đã hủy
    }

    public static class OrderStatusExtensions
    {
        public static string ToVietnamese(this OrderStatus status) => status switch
        {
            OrderStatus.Pending   => "Chờ xử lý",
            OrderStatus.Brewing   => "Đang pha chế",
            OrderStatus.Ready     => "Sẵn sàng",
            OrderStatus.Delivered => "Đã giao",
            OrderStatus.Cancelled => "Đã hủy",
            _                     => status.ToString()
        };

        public static string ToBadgeClass(this OrderStatus status) => status switch
        {
            OrderStatus.Pending   => "badge-pending",
            OrderStatus.Brewing   => "badge-brewing",
            OrderStatus.Ready     => "badge-ready",
            OrderStatus.Delivered => "badge-delivered",
            OrderStatus.Cancelled => "badge-cancelled",
            _                     => "badge-pending"
        };
    }
}
