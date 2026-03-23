using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SaleStore.Data;
using SaleStore.Models;

namespace SaleStore.Pages
{
    public class OrderSuccessModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public OrderSuccessModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public Order? Order { get; set; }
        public Customer? Customer { get; set; }

        public IActionResult OnGet(int orderId)
        {
            Order = _context.Orders
                .Include(o => o.Customer)
                .Where(o => o.Id == orderId)
                .FirstOrDefault();

            if (Order == null)
            {
                return RedirectToPage("/Index");
            }

            Customer = Order.Customer;

            return Page();
        }
    }
}
