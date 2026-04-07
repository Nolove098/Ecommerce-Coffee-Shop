using SaleStore.Data; // Đảm bảo gọi đúng namespace chứa ApplicationDbContext
using SaleStore.Hubs;
using SaleStore.Middleware;
using SaleStore.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

var builder = WebApplication.CreateBuilder(args);

// 1. THÊM MỚI: Cấu hình kết nối DbContext với Supabase (PostgreSQL)
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") 
    ?? "Host=15.164.188.235;Port=5432;Database=postgres;Username=postgres.uxlkglkfpmtpvruqndqx;Password=Baospaki1234@;SSL Mode=Require;Trust Server Certificate=true;";

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(connectionString)
           .UseSnakeCaseNamingConvention());

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Auth/Login";
        options.AccessDeniedPath = "/Auth/AccessDenied";
        options.Cookie.Name = "salestore.auth";
        options.SlidingExpiration = true;
        options.ExpireTimeSpan = TimeSpan.FromDays(14);
        options.Events = new CookieAuthenticationEvents
        {
            OnValidatePrincipal = async context =>
            {
                var userIdClaim = context.Principal?.FindFirstValue(ClaimTypes.NameIdentifier);
                if (!long.TryParse(userIdClaim, out var userId))
                {
                    context.RejectPrincipal();
                    await context.HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
                    return;
                }

                var db = context.HttpContext.RequestServices.GetRequiredService<ApplicationDbContext>();
                var user = await db.AppUsers.AsNoTracking().FirstOrDefaultAsync(x => x.Id == userId);
                if (user == null || !user.IsActive)
                {
                    context.RejectPrincipal();
                    await context.HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
                    return;
                }

                var roleClaim = context.Principal?.FindFirstValue(ClaimTypes.Role);
                if (!string.Equals(roleClaim, user.Role, StringComparison.Ordinal))
                {
                    context.RejectPrincipal();
                    await context.HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
                }
            }
        };
    });

builder.Services.AddAuthorization();
builder.Services.AddSingleton<PasswordHasher>();
builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();

// Payment Services
builder.Services.AddScoped<IVnPayService, VnPayService>();

// ChatBot Service
builder.Services.AddSingleton<IChatBotService, GeminiChatService>();

// 2. Session để hỗ trợ giỏ hàng (Giữ nguyên của bạn)
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(60);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    var passwordHasher = scope.ServiceProvider.GetRequiredService<PasswordHasher>();
    await AuthDbInitializer.EnsureCreatedAsync(dbContext, passwordHasher);
}

app.UseRequestLogging();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.UseSession();

// 3. Route cho Area "Admin" (Giữ nguyên của bạn)
app.MapControllerRoute(
    name: "admin",
    pattern: "Admin/{controller=Dashboard}/{action=Index}/{id?}",
    defaults: new { area = "Admin" },
    constraints: new { },
    dataTokens: new { area = "Admin" });

app.MapControllerRoute(
    name: "staff",
    pattern: "Staff/{controller=POS}/{action=Index}/{id?}",
    defaults: new { area = "Staff" },
    constraints: new { },
    dataTokens: new { area = "Staff" });

// 4. Route mặc định (Giữ nguyên của bạn)
app.MapDefaultControllerRoute();
app.MapRazorPages();
app.MapBlazorHub();
app.MapHub<OrderHub>("/hubs/order");

app.Run();