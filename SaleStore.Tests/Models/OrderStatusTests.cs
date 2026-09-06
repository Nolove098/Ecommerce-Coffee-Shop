using SaleStore.Models;

namespace SaleStore.Tests.Models;

public class OrderStatusTests
{
    [Theory]
    [InlineData(OrderStatus.Pending, "Chờ xử lý", "badge-pending")]
    [InlineData(OrderStatus.Ready, "Sẵn sàng", "badge-ready")]
    [InlineData(OrderStatus.Delivered, "Đã giao", "badge-delivered")]
    [InlineData(OrderStatus.Cancelled, "Đã hủy", "badge-cancelled")]
    public void Display_WhenKnownStatus_UsesVietnameseLabelAndMatchingBadge(OrderStatus status, string label, string badge)
    {
        Assert.Equal(label, status.ToVietnamese());
        Assert.Equal(badge, status.ToBadgeClass());
    }

    [Theory]
    [InlineData(1)]
    [InlineData(-1)]
    [InlineData(99)]
    public void Display_WhenStatusIsUnknown_UsesPendingFallback(int value)
    {
        Assert.Equal("Chờ xử lý", ((OrderStatus)value).ToVietnamese());
        Assert.Equal("badge-pending", ((OrderStatus)value).ToBadgeClass());
    }
}
