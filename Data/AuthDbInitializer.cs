using Microsoft.EntityFrameworkCore;
using SaleStore.Models;
using SaleStore.Services;

namespace SaleStore.Data
{
    public static class AuthDbInitializer
    {
        public static async Task EnsureCreatedAsync(
            ApplicationDbContext context,
            PasswordHasher passwordHasher,
            IConfiguration configuration)
        {
            await BootstrapUserAsync(context, passwordHasher, configuration, "BootstrapAdmin", AppRoles.Admin);
            await BootstrapUserAsync(context, passwordHasher, configuration, "BootstrapStaff", AppRoles.Staff);

            if (!configuration.GetValue<bool>("DataInitialization:EnableDemoData"))
                return;

            // Seed sản phẩm "Trà & Khác" nếu chưa có
            var hasTeaProducts = await context.Products.AnyAsync(p => p.Category == "Trà & Khác");
            if (!hasTeaProducts)
            {
                var teaProducts = new[]
                {
                    new Product
                    {
                        Name = "Trà Sen Vàng",
                        Description = "Trà ướp hương sen thanh mát, vị ngọt nhẹ tự nhiên — thức uống truyền thống Việt Nam.",
                        Price = 45000,
                        Category = "Trà & Khác",
                        ImageUrl = "https://images.unsplash.com/photo-1564890369478-c89ca6d9cde9?w=600&h=400&fit=crop",
                        Stock = 100,
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    },
                    new Product
                    {
                        Name = "Trà Đào Cam Sả",
                        Description = "Trà đào kết hợp cam tươi và sả thơm, tươi mát cho ngày nắng.",
                        Price = 50000,
                        Category = "Trà & Khác",
                        ImageUrl = "https://images.unsplash.com/photo-1556679343-c7306c1976bc?w=600&h=400&fit=crop",
                        Stock = 80,
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    },
                    new Product
                    {
                        Name = "Trà Matcha Latte",
                        Description = "Matcha Nhật Bản nguyên chất kết hợp sữa tươi, béo ngậy và thơm dịu.",
                        Price = 55000,
                        Category = "Trà & Khác",
                        ImageUrl = "https://images.unsplash.com/photo-1515823064-d6e0c04616a7?w=600&h=400&fit=crop",
                        Stock = 60,
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    },
                    new Product
                    {
                        Name = "Nước Ép Cam Tươi",
                        Description = "Cam vắt tươi 100%, không thêm đường — bổ sung vitamin C tự nhiên.",
                        Price = 40000,
                        Category = "Trà & Khác",
                        ImageUrl = "https://images.unsplash.com/photo-1621506289937-a8e4df240d0b?w=600&h=400&fit=crop",
                        Stock = 50,
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    }
                };

                context.Products.AddRange(teaProducts);
                await context.SaveChangesAsync();
            }
        }

        private static async Task BootstrapUserAsync(
            ApplicationDbContext context,
            PasswordHasher passwordHasher,
            IConfiguration configuration,
            string sectionName,
            string role)
        {
            var username = configuration[$"{sectionName}:Username"]?.Trim();
            var fullName = configuration[$"{sectionName}:FullName"]?.Trim();
            var email = configuration[$"{sectionName}:Email"]?.Trim().ToLowerInvariant();
            var password = configuration[$"{sectionName}:Password"];

            if (string.IsNullOrWhiteSpace(username) ||
                string.IsNullOrWhiteSpace(email) ||
                string.IsNullOrWhiteSpace(password))
            {
                return;
            }

            var existingByEmail = await context.AppUsers.FirstOrDefaultAsync(x => x.Email == email);
            if (existingByEmail != null)
                return;

            // Keep the configured login email usable when a protected bootstrap
            // account already exists under the configured username. This is a
            // narrow metadata reconciliation: credentials and roles are untouched.
            var existingByUsername = await context.AppUsers.FirstOrDefaultAsync(x => x.Username == username);
            if (existingByUsername != null)
                return;

            var (hash, salt) = passwordHasher.CreateHash(password);
            context.AppUsers.Add(new AppUser
            {
                Username = username,
                FullName = string.IsNullOrWhiteSpace(fullName) ? username : fullName,
                Email = email,
                PasswordHash = hash,
                PasswordSalt = salt,
                Role = role,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            });

            await context.SaveChangesAsync();
        }
    }
}
