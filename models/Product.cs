using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SaleStore.Models
{
    public class Product
    {
        public long ProductID { get; set; }

        [Required(ErrorMessage = "Tên sản phẩm là bắt buộc")]
        [Display(Name = "Tên sản phẩm")]
        public string Name { get; set; } = string.Empty;

        [Display(Name = "Mô tả")]
        public string Description { get; set; } = string.Empty;

        [Required(ErrorMessage = "Giá là bắt buộc")]
        [Column(TypeName = "decimal(8, 2)")]
        [Display(Name = "Giá (VNĐ)")]
        [Range(0, 99999999, ErrorMessage = "Giá không hợp lệ")]
        public decimal Price { get; set; }

        [Required(ErrorMessage = "Danh mục là bắt buộc")]
        [Display(Name = "Danh mục")]
        public string Category { get; set; } = string.Empty;

        [Display(Name = "Ảnh URL")]
        public string ImageUrl { get; set; } = string.Empty;

        [Display(Name = "Tồn kho")]
        [Range(0, int.MaxValue, ErrorMessage = "Số lượng không hợp lệ")]
        public int Stock { get; set; } = 0;

        [Display(Name = "Đang bán")]
        public bool IsActive { get; set; } = true;
    }
}
