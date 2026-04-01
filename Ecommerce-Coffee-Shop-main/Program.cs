using SaleStore.Data; // Đảm bảo gọi đúng namespace chứa ApplicationDbContext
using SaleStore.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// 1. THÊM MỚI: Cấu hình kết nối DbContext với Supabase (PostgreSQL)
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"))
           .UseSnakeCaseNamingConvention());

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Auth/Login";
        options.AccessDeniedPath = "/Auth/AccessDenied";
        options.Cookie.Name = "salestore.auth";
        options.SlidingExpiration = true;
        options.ExpireTimeSpan = TimeSpan.FromDays(14);
    });

builder.Services.AddAuthorization();
builder.Services.AddSingleton<PasswordHasher>();
builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();

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
    
    // Apply pending migrations
    await dbContext.Database.MigrateAsync();
    
    await AuthDbInitializer.EnsureCreatedAsync(dbContext, passwordHasher);
}

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

app.Run();