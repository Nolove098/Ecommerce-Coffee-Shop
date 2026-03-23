using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace SaleStore.Pages
{
    public class ContactModel : PageModel
    {
        [BindProperty]
        public ContactForm? Form { get; set; }

        public string? SuccessMessage { get; set; }

        public void OnGet()
        {
            ViewData["Title"] = "Liên hệ - Cửa hàng Cà phê";
        }

        public IActionResult OnPost()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            try
            {
                // TODO: Implement email sending or database storage
                // For now, just show success message
                SuccessMessage = $"Cảm ơn {Form.Name}! Chúng tôi đã nhận được tin nhắn của bạn và sẽ liên lạc trong 24 giờ.";
                
                // Clear form
                Form = new ContactForm();
                
                return Page();
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Có lỗi xảy ra: {ex.Message}");
                return Page();
            }
        }
    }

    public class ContactForm
    {
        public string? Name { get; set; }
        public string? Email { get; set; }
        public string? Phone { get; set; }
        public string? Subject { get; set; }
        public string? Message { get; set; }
    }
}
