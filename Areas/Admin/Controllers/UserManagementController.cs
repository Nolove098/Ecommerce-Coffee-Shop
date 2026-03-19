using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SaleStore.Data;
using SaleStore.Models;
using SaleStore.Models.ViewModels;
using SaleStore.Services;

namespace SaleStore.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = AppRoles.Admin)]
    public class UserManagementController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly PasswordHasher _passwordHasher;

        public UserManagementController(ApplicationDbContext context, PasswordHasher passwordHasher)
        {
            _context = context;
            _passwordHasher = passwordHasher;
        }

        public async Task<IActionResult> Index()
        {
            var users = await _context.AppUsers
                .OrderByDescending(x => x.Role == AppRoles.Admin)
                .ThenByDescending(x => x.Role == AppRoles.Staff)
                .ThenBy(x => x.Username)
                .ToListAsync();

            return View(users);
        }

        [HttpGet]
        public IActionResult CreateStaff()
        {
            return View(new AdminCreateStaffViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateStaff(AdminCreateStaffViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var normalizedUsername = model.Username.Trim().ToLowerInvariant();
            var normalizedEmail = model.Email.Trim().ToLowerInvariant();

            if (await _context.AppUsers.AnyAsync(x => x.Username == normalizedUsername))
            {
                ModelState.AddModelError(nameof(model.Username), "Tên đăng nhập đã tồn tại.");
                return View(model);
            }

            if (await _context.AppUsers.AnyAsync(x => x.Email == normalizedEmail))
            {
                ModelState.AddModelError(nameof(model.Email), "Email đã tồn tại.");
                return View(model);
            }

            var (hash, salt) = _passwordHasher.CreateHash(model.Password);
            _context.AppUsers.Add(new AppUser
            {
                Username = normalizedUsername,
                FullName = model.FullName.Trim(),
                Email = normalizedEmail,
                Phone = string.IsNullOrWhiteSpace(model.Phone) ? null : model.Phone.Trim(),
                PasswordHash = hash,
                PasswordSalt = salt,
                Role = AppRoles.Staff,
                IsActive = model.IsActive,
                CreatedAt = DateTime.UtcNow
            });

            await _context.SaveChangesAsync();
            TempData["Success"] = $"Đã tạo tài khoản nhân viên '{normalizedUsername}'.";
            return RedirectToAction(nameof(Index));
        }
    }
}