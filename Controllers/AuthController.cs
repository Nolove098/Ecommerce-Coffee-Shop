using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SaleStore.Data;
using SaleStore.Models;
using SaleStore.Models.ViewModels;
using SaleStore.Services;
using System.Security.Claims;

namespace SaleStore.Controllers
{
    public class AuthController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly PasswordHasher _passwordHasher;

        public AuthController(ApplicationDbContext context, PasswordHasher passwordHasher)
        {
            _context = context;
            _passwordHasher = passwordHasher;
        }

        [AllowAnonymous]
        [HttpGet]
        [Route("Auth/Login")]
        [Route("Admin/Auth/Login")]
        public IActionResult Login(string? returnUrl = null)
        {
            if (User.Identity?.IsAuthenticated == true)
                return RedirectByRole();

            return View(new LoginViewModel { ReturnUrl = returnUrl });
        }

        [AllowAnonymous]
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("Auth/Login")]
        [Route("Admin/Auth/Login")]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var normalizedLoginId = NormalizeLoginId(model.LoginId);
            var user = await _context.AppUsers.FirstOrDefaultAsync(x => x.Email == normalizedLoginId || x.Username == normalizedLoginId);

            if (user == null || !_passwordHasher.Verify(model.Password, user.PasswordHash, user.PasswordSalt))
            {
                ModelState.AddModelError(string.Empty, "Email hoặc mật khẩu không chính xác.");
                return View(model);
            }

            if (!user.IsActive)
            {
                ModelState.AddModelError(string.Empty, "Tài khoản của bạn hiện đang bị khóa.");
                return View(model);
            }

            user.LastLoginAt = DateTime.UtcNow;
            user.UpdatedAt = DateTime.UtcNow;

            _context.AuthActivities.Add(new AuthActivity
            {
                UserId = user.Id,
                ActivityType = "login",
                IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString(),
                UserAgent = Request.Headers.UserAgent.ToString(),
                CreatedAt = DateTime.UtcNow
            });

            await _context.SaveChangesAsync();
            await SignInUserAsync(user, model.RememberMe);

            if (!string.IsNullOrWhiteSpace(model.ReturnUrl) && Url.IsLocalUrl(model.ReturnUrl))
                return Redirect(model.ReturnUrl);

            return RedirectByRole(user.Role);
        }

        [AllowAnonymous]
        [HttpGet]
        [Route("Auth/Register")]
        [Route("Admin/Auth/Register")]
        public IActionResult Register()
        {
            if (User.Identity?.IsAuthenticated == true)
                return RedirectByRole();

            return View(new RegisterViewModel());
        }

        [AllowAnonymous]
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("Auth/Register")]
        [Route("Admin/Auth/Register")]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var normalizedEmail = NormalizeEmail(model.Email);
            var emailExists = await _context.AppUsers.AnyAsync(x => x.Email == normalizedEmail);
            if (emailExists)
            {
                ModelState.AddModelError(nameof(model.Email), "Email này đã được sử dụng.");
                return View(model);
            }

            var (hash, salt) = _passwordHasher.CreateHash(model.Password);
            var user = new AppUser
            {
                Username = await GenerateUniqueUsernameAsync(model.Email, model.FullName),
                FullName = model.FullName.Trim(),
                Email = normalizedEmail,
                Phone = string.IsNullOrWhiteSpace(model.Phone) ? null : model.Phone.Trim(),
                PasswordHash = hash,
                PasswordSalt = salt,
                Role = AppRoles.User,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            _context.AppUsers.Add(user);
            await _context.SaveChangesAsync();

            _context.AuthActivities.Add(new AuthActivity
            {
                UserId = user.Id,
                ActivityType = "register",
                IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString(),
                UserAgent = Request.Headers.UserAgent.ToString(),
                CreatedAt = DateTime.UtcNow
            });

            await _context.SaveChangesAsync();
            await SignInUserAsync(user, false);

            return RedirectToAction("Index", "Home", new { area = "" });
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("Auth/Logout")]
        [Route("Admin/Auth/Logout")]
        public async Task<IActionResult> Logout()
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (long.TryParse(userIdClaim, out var userId))
            {
                _context.AuthActivities.Add(new AuthActivity
                {
                    UserId = userId,
                    ActivityType = "logout",
                    IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString(),
                    UserAgent = Request.Headers.UserAgent.ToString(),
                    CreatedAt = DateTime.UtcNow
                });

                await _context.SaveChangesAsync();
            }

            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Index", "Home", new { area = "" });
        }

        [AllowAnonymous]
        [HttpGet]
        [Route("Auth/AccessDenied")]
        public IActionResult AccessDenied()
        {
            return RedirectByRole();
        }

        private IActionResult RedirectByRole(string? role = null)
        {
            var resolvedRole = role ?? User.FindFirstValue(ClaimTypes.Role);

            return resolvedRole switch
            {
                AppRoles.Admin => RedirectToAction("Index", "Dashboard", new { area = "Admin" }),
                AppRoles.Staff => RedirectToAction("Index", "POS", new { area = "Staff" }),
                _ => RedirectToAction("Index", "Home", new { area = "" })
            };
        }

        private async Task SignInUserAsync(AppUser user, bool isPersistent)
        {
            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new(ClaimTypes.Name, user.FullName),
                new(ClaimTypes.Email, user.Email),
                new(ClaimTypes.Role, user.Role)
            };

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                principal,
                new AuthenticationProperties
                {
                    IsPersistent = isPersistent,
                    ExpiresUtc = isPersistent ? DateTimeOffset.UtcNow.AddDays(14) : null
                });
        }

        private static string NormalizeEmail(string email)
        {
            return email.Trim().ToLowerInvariant();
        }

        private static string NormalizeLoginId(string loginId)
        {
            return loginId.Trim().ToLowerInvariant();
        }

        private async Task<string> GenerateUniqueUsernameAsync(string email, string fullName)
        {
            var emailLocalPart = email.Split('@', StringSplitOptions.RemoveEmptyEntries).FirstOrDefault();
            var seed = string.IsNullOrWhiteSpace(emailLocalPart) ? fullName : emailLocalPart;
            var normalizedSeed = new string(seed.Trim().ToLowerInvariant().Where(char.IsLetterOrDigit).ToArray());
            var baseUsername = string.IsNullOrWhiteSpace(normalizedSeed) ? "user" : normalizedSeed;
            var candidate = baseUsername;
            var suffix = 1;

            while (await _context.AppUsers.AnyAsync(x => x.Username == candidate))
            {
                suffix++;
                candidate = $"{baseUsername}{suffix}";
            }

            return candidate;
        }
    }
}