using SaleStore.Data;
using SaleStore.Hubs;
using SaleStore.Middleware;
using SaleStore.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using System.Security.Claims;

var builder = WebApplication.CreateBuilder(args);

// Railway and Render expose the assigned listener through PORT. Local
// development continues to use launchSettings/--urls when PORT is absent.
if (int.TryParse(builder.Configuration["PORT"], out var platformPort) &&
    platformPort is > 0 and <= 65535)
{
    builder.WebHost.UseUrls($"http://0.0.0.0:{platformPort}");
}

builder.Logging.ClearProviders();
builder.Logging.AddConsole();
if (builder.Environment.IsDevelopment())
    builder.Logging.AddDebug();

var connectionString = PostgresConnectionString.Normalize(
    builder.Configuration.GetConnectionString("DefaultConnection"),
    builder.Configuration.GetValue<bool>("Supabase:ForceSessionPooler"),
    builder.Configuration["Supabase:ProjectRef"],
    builder.Configuration["Supabase:PoolerRegion"]);

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(connectionString)
           .UseSnakeCaseNamingConvention());

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Auth/Login";
        options.AccessDeniedPath = "/Auth/AccessDenied";
        options.Cookie.Name = "salestore.auth";
        options.Cookie.HttpOnly = true;
        options.Cookie.SameSite = SameSiteMode.Lax;
        options.Cookie.SecurePolicy = builder.Environment.IsDevelopment()
            ? CookieSecurePolicy.SameAsRequest
            : CookieSecurePolicy.Always;
        options.SlidingExpiration = true;
        options.ExpireTimeSpan = TimeSpan.FromDays(14);
        options.Events = new CookieAuthenticationEvents
        {
            OnRedirectToLogin = context =>
            {
                if (context.Request.Path.StartsWithSegments("/api") ||
                    context.Request.Path.Equals("/Auth/CurrentUser", StringComparison.OrdinalIgnoreCase))
                {
                    context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                    context.Response.ContentType = "application/json; charset=utf-8";
                    return context.Response.WriteAsJsonAsync(new { authenticated = false, role = (string?)null });
                }

                context.Response.Redirect(context.RedirectUri);
                return Task.CompletedTask;
            },
            OnRedirectToAccessDenied = context =>
            {
                if (context.Request.Path.StartsWithSegments("/api"))
                {
                    context.Response.StatusCode = StatusCodes.Status403Forbidden;
                    context.Response.ContentType = "application/json; charset=utf-8";
                    return context.Response.WriteAsJsonAsync(new { error = "Forbidden" });
                }

                context.Response.Redirect(context.RedirectUri);
                return Task.CompletedTask;
            },
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
builder.Services.AddSignalR();
builder.Services.AddHealthChecks();
builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
    options.ForwardLimit = 1;

    // Explicit opt-in for managed cloud reverse proxies whose internal
    // addresses are not stable. Kestrel is not exposed directly there.
    if (builder.Configuration.GetValue<bool>("ASPNETCORE_FORWARDEDHEADERS_ENABLED"))
    {
        options.KnownIPNetworks.Clear();
        options.KnownProxies.Clear();
    }
});

builder.Services.AddScoped<IVnPayService, VnPayService>();
builder.Services.AddHttpClient<IChatBotService, GeminiChatService>();
builder.Services.AddSingleton<SalesForecastService>();
builder.Services.AddSingleton<ProductRecommendService>();

builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(60);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.Cookie.SameSite = SameSiteMode.Lax;
    options.Cookie.SecurePolicy = builder.Environment.IsDevelopment()
        ? CookieSecurePolicy.SameAsRequest
        : CookieSecurePolicy.Always;
});

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    var passwordHasher = scope.ServiceProvider.GetRequiredService<PasswordHasher>();
    if (builder.Configuration.GetValue<bool>("DataInitialization:EnableBootstrapUsers"))
        await AuthDbInitializer.EnsureCreatedAsync(dbContext, passwordHasher, builder.Configuration);
    if (builder.Configuration.GetValue<bool>("DemoSeed:Enabled"))
        await DeterministicDemoSeeder.SeedAsync(dbContext, builder.Configuration);
}

app.UseForwardedHeaders();

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
    app.UseHttpsRedirection();
}

app.UseHealthChecks("/health");
app.UseRequestLogging();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.UseSession();

app.MapStaticAssets();

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

app.MapDefaultControllerRoute();
app.MapRazorPages();
app.MapBlazorHub();
app.MapHub<OrderHub>("/hubs/order");

app.Run();
