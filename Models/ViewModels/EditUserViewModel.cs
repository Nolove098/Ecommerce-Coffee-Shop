using System.ComponentModel.DataAnnotations;

namespace SaleStore.Models.ViewModels
{
    public class EditUserViewModel
    {
        public long Id { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập họ tên.")]
        [StringLength(120)]
        [Display(Name = "Họ tên")]
        public string FullName { get; set; } = null!;

        [Required(ErrorMessage = "Vui lòng nhập email.")]
        [EmailAddress(ErrorMessage = "Email không hợp lệ.")]
        [StringLength(180)]
        [Display(Name = "Email")]
        public string Email { get; set; } = null!;

        [StringLength(30)]
        [Display(Name = "Số điện thoại")]
        public string? Phone { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn vai trò.")]
        [Display(Name = "Vai trò")]
        public string Role { get; set; } = AppRoles.User;

        [Display(Name = "Kích hoạt")]
        public bool IsActive { get; set; } = true;

        [StringLength(100, MinimumLength = 8, ErrorMessage = "Mật khẩu phải có ít nhất 8 ký tự.")]
        [Display(Name = "Mật khẩu mới (để trống nếu không đổi)")]
        public string? NewPassword { get; set; }

        public string Username { get; set; } = null!;
    }
}
