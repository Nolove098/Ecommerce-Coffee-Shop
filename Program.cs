using SaleStore.Data; // Đảm bảo gọi đúng namespace chứa ApplicationDbContext
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// 1. THÊM MỚI: Cấu hình kết nối DbContext với Supabase (PostgreSQL)
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"))
           .UseSnakeCaseNamingConvention());

builder.Services.AddControllersWithViews();

// 2. Session để hỗ trợ giỏ hàng (Giữ nguyên của bạn)
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(60);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

var app = builder.Build();

app.UseStaticFiles();
app.UseRouting();
app.UseSession();

// 3. Route cho Area "Admin" (Giữ nguyên của bạn)
app.MapControllerRoute(
    name: "admin",
    pattern: "Admin/{controller=Dashboard}/{action=Index}/{id?}",
    defaults: new { area = "Admin" },
    constraints: new { },
    dataTokens: new { area = "Admin" });

// 4. Route mặc định (Giữ nguyên của bạn)
app.MapDefaultControllerRoute();

app.Run();