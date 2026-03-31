using System.ComponentModel.DataAnnotations;
using SaleStore.Models.Validation;

namespace SaleStore.Models.ViewModels;

public class RegisterViewModel
{
    [Required(ErrorMessage = "Vui lòng nhập họ và tên.")]
    [StringLength(120, MinimumLength = 2, ErrorMessage = "Họ tên phải có từ 2 đến 120 ký tự.")]
    [Display(Name = "Họ và tên")]
    public string FullName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Vui lòng nhập email.")]
    [EmailAddress(ErrorMessage = "Email không đúng định dạng.")]
    [Display(Name = "Email")]
    public string Email { get; set; } = string.Empty;

    [RegularExpression(@"^[0-9+\-\s()]{9,30}$", ErrorMessage = "Số điện thoại chỉ gồm số và các ký tự + - ( ) khoảng trắng, độ dài 9-30 ký tự.")]
    [StringLength(30, MinimumLength = 9, ErrorMessage = "Số điện thoại phải từ 9 đến 30 ký tự.")]
    [Display(Name = "Số điện thoại")]
    public string? Phone { get; set; }

    [Required(ErrorMessage = "Vui lòng nhập mật khẩu.")]
    [StringLength(100, MinimumLength = 8, ErrorMessage = "Mật khẩu phải có ít nhất 8 ký tự.")]
    [StrongPassword]
    [DataType(DataType.Password)]
    [Display(Name = "Mật khẩu")]
    public string Password { get; set; } = string.Empty;

    [Required(ErrorMessage = "Vui lòng xác nhận mật khẩu.")]
    [DataType(DataType.Password)]
    [Compare(nameof(Password), ErrorMessage = "Mật khẩu xác nhận không khớp.")]
    [Display(Name = "Xác nhận mật khẩu")]
    public string ConfirmPassword { get; set; } = string.Empty;
}