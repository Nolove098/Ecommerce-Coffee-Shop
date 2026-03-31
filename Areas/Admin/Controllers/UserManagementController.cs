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

        [HttpGet]
        public async Task<IActionResult> Edit(long id)
        {
            var user = await _context.AppUsers.FindAsync(id);
            if (user == null) return NotFound();

            var vm = new EditUserViewModel
            {
                Id = user.Id,
                Username = user.Username,
                FullName = user.FullName,
                Email = user.Email,
                Phone = user.Phone,
                Role = user.Role,
                IsActive = user.IsActive
            };
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(EditUserViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var user = await _context.AppUsers.FindAsync(model.Id);
            if (user == null) return NotFound();

            // Prevent admin from demoting themselves
            var currentUsername = User.Identity?.Name?.ToLowerInvariant();
            if (user.Username == currentUsername && model.Role != AppRoles.Admin)
            {
                ModelState.AddModelError(nameof(model.Role), "Bạn không thể thay đổi vai trò của chính mình.");
                model.Username = user.Username;
                return View(model);
            }

            var normalizedEmail = model.Email.Trim().ToLowerInvariant();
            if (await _context.AppUsers.AnyAsync(x => x.Email == normalizedEmail && x.Id != model.Id))
            {
                ModelState.AddModelError(nameof(model.Email), "Email đã tồn tại.");
                model.Username = user.Username;
                return View(model);
            }

            user.FullName = model.FullName.Trim();
            user.Email = normalizedEmail;
            user.Phone = string.IsNullOrWhiteSpace(model.Phone) ? null : model.Phone.Trim();
            user.Role = model.Role;
            user.IsActive = model.IsActive;
            user.UpdatedAt = DateTime.UtcNow;

            if (!string.IsNullOrWhiteSpace(model.NewPassword))
            {
                var (hash, salt) = _passwordHasher.CreateHash(model.NewPassword);
                user.PasswordHash = hash;
                user.PasswordSalt = salt;
            }

            await _context.SaveChangesAsync();
            TempData["Success"] = $"Đã cập nhật tài khoản '{user.Username}'.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleActive(long id)
        {
            var user = await _context.AppUsers.FindAsync(id);
            if (user == null) return NotFound();

            var currentUsername = User.Identity?.Name?.ToLowerInvariant();
            if (user.Username == currentUsername)
            {
                TempData["Error"] = "Bạn không thể khóa tài khoản của chính mình.";
                return RedirectToAction(nameof(Index));
            }

            user.IsActive = !user.IsActive;
            user.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            TempData["Success"] = user.IsActive
                ? $"Đã mở khóa tài khoản '{user.Username}'."
                : $"Đã khóa tài khoản '{user.Username}'.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(long id)
        {
            var user = await _context.AppUsers.FindAsync(id);
            if (user == null) return NotFound();

            var currentUsername = User.Identity?.Name?.ToLowerInvariant();
            if (user.Username == currentUsername)
            {
                TempData["Error"] = "Bạn không thể xóa tài khoản của chính mình.";
                return RedirectToAction(nameof(Index));
            }

            _context.AppUsers.Remove(user);
            await _context.SaveChangesAsync();

            TempData["Success"] = $"Đã xóa tài khoản '{user.Username}'.";
            return RedirectToAction(nameof(Index));
        }
    }
}