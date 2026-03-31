using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SaleStore.Data;
using SaleStore.Models;
using System.Security.Claims;

namespace SaleStore.Controllers;

[Authorize]
public class OrderController : Controller
{
    private readonly ApplicationDbContext _context;

    public OrderController(ApplicationDbContext context)
    {
        _context = context;
    }

    // GET: /Order/History
    [HttpGet]
    [Route("Order/History")]
    public async Task<IActionResult> History()
    {
        var email = User.FindFirstValue(ClaimTypes.Email);
        if (string.IsNullOrEmpty(email))
            return RedirectToAction("Login", "Auth");

        // Tìm customer theo email → phone match qua AppUser
        var user = await _context.AppUsers.FirstOrDefaultAsync(u => u.Email == email);
        if (user == null)
            return RedirectToAction("Index", "Home");

        // Tìm customer bằng phone hoặc tên
        var phone = user.Phone;
        var orders = new List<Order>();

        if (!string.IsNullOrWhiteSpace(phone))
        {
            var customer = await _context.Customers.FirstOrDefaultAsync(c => c.Phone == phone);
            if (customer != null)
            {
                orders = await _context.Orders
                    .Include(o => o.OrderItems)
                    .Where(o => o.CustomerId == customer.Id)
                    .OrderByDescending(o => o.CreatedAt)
                    .ToListAsync();
            }
        }

        return View(orders);
    }

    // GET: /Order/Detail/{id}
    [HttpGet]
    [Route("Order/Detail/{id:long}")]
    public async Task<IActionResult> Detail(long id)
    {
        var email = User.FindFirstValue(ClaimTypes.Email);
        if (string.IsNullOrEmpty(email))
            return RedirectToAction("Login", "Auth");

        var user = await _context.AppUsers.FirstOrDefaultAsync(u => u.Email == email);
        if (user == null)
            return RedirectToAction("Index", "Home");

        var phone = user.Phone;
        if (string.IsNullOrWhiteSpace(phone))
            return NotFound();

        var customer = await _context.Customers.FirstOrDefaultAsync(c => c.Phone == phone);
        if (customer == null)
            return NotFound();

        var order = await _context.Orders
            .Include(o => o.Customer)
            .Include(o => o.OrderItems)
            .FirstOrDefaultAsync(o => o.Id == id && o.CustomerId == customer.Id);

        if (order == null)
            return NotFound();

        return View(order);
    }
}
