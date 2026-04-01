using System.ComponentModel.DataAnnotations;

namespace SaleStore.Models.ViewModels;

public class CheckoutViewModel
{
    [Required(ErrorMessage = "Vui lòng nhập họ tên")]
    [Display(Name = "Họ và tên")]
    public string CustomerName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Vui lòng nhập số điện thoại")]
    [Display(Name = "Số điện thoại")]
    [Phone(ErrorMessage = "Số điện thoại không hợp lệ")]
    public string CustomerPhone { get; set; } = string.Empty;

    [Display(Name = "Địa chỉ giao hàng")]
    public string? Address { get; set; }

    [Display(Name = "Ghi chú")]
    public string? Note { get; set; }

    // Cart items passed as JSON from localStorage
    public string CartJson { get; set; } = "[]";
}
