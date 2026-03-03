var builder = WebApplication.CreateBuilder(args);

// Tác dụng là đăng ký các dịch vụ cần thiết cho ứng dụng MVC
builder.Services.AddControllersWithViews();

var app = builder.Build();

// Cấu hình để ứng dụng có thể phục vụ các tệp tĩnh như CSS, JavaScript, hình ảnh, v.v.
app.UseStaticFiles();

app.UseRouting();

// Route cho Area "Admin"  (/Admin/Controller/Action)
app.MapControllerRoute(
    name: "admin",
    pattern: "Admin/{controller=Dashboard}/{action=Index}/{id?}",
    defaults: new { area = "Admin" },
    constraints: new { },
    dataTokens: new { area = "Admin" });

// Route mặc định cho toàn bộ app
app.MapDefaultControllerRoute();

app.Run();
