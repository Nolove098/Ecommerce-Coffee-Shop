using Microsoft.EntityFrameworkCore;
using SaleStore.Models;

namespace SaleStore.Data;

/// <summary>
/// Seed nhiều sản phẩm + dữ liệu đơn hàng mẫu để ML.NET hoạt động tốt.
/// Chạy 1 lần duy nhất (kiểm tra flag trước khi seed).
/// </summary>
public static class MlDataSeeder
{
    public static async Task SeedAsync(ApplicationDbContext context)
    {
        // ────── 1. SEED SẢN PHẨM ──────
        // Kiểm tra nếu đã có nhiều sản phẩm thì bỏ qua
        var productCount = await context.Products.CountAsync();
        if (productCount < 15)
        {
            var newProducts = GetProducts();
            foreach (var p in newProducts)
            {
                var exists = await context.Products.AnyAsync(x => x.Name == p.Name);
                if (!exists)
                    context.Products.Add(p);
            }
            await context.SaveChangesAsync();
        }

        // ────── 1b. CẬP NHẬT ẢNH SẢN PHẨM (fix ảnh lỗi) ──────
        await UpdateProductImagesAsync(context);

        // ────── 2. SEED KHÁCH HÀNG MẪU ──────
        var customerCount = await context.Customers.CountAsync();
        if (customerCount < 10)
        {
            var customers = GetCustomers();
            foreach (var c in customers)
            {
                var exists = await context.Customers.AnyAsync(x => x.Phone == c.Phone);
                if (!exists)
                    context.Customers.Add(c);
            }
            await context.SaveChangesAsync();
        }

        // ────── 3. SEED ĐƠN HÀNG MẪU CHO ML ──────
        var orderCount = await context.Orders.CountAsync();
        if (orderCount >= 80)
            return; // Đã có đủ dữ liệu

        var allProducts = await context.Products.Where(p => p.IsActive).ToListAsync();
        // Chỉ dùng khách hàng mẫu (phone 0901000xxx), KHÔNG dùng khách hàng thật
        var seededPhones = GetCustomers().Select(c => c.Phone).ToHashSet();
        var allCustomers = await context.Customers
            .Where(c => seededPhones.Contains(c.Phone))
            .ToListAsync();

        if (!allProducts.Any() || !allCustomers.Any())
            return;

        var rng = new Random(42); // Fixed seed for reproducibility
        var now = DateTime.UtcNow;
        var orders = new List<Order>();

        // Tạo đơn hàng trong 90 ngày qua với pattern thực tế
        for (int dayOffset = 90; dayOffset >= 0; dayOffset--)
        {
            var date = now.Date.AddDays(-dayOffset);

            // Số đơn mỗi ngày: nhiều hơn vào cuối tuần, ít hơn đầu tuần
            var dayOfWeek = date.DayOfWeek;
            int baseOrders = dayOfWeek switch
            {
                DayOfWeek.Saturday => rng.Next(4, 8),
                DayOfWeek.Sunday => rng.Next(3, 7),
                DayOfWeek.Friday => rng.Next(3, 6),
                _ => rng.Next(1, 5)
            };

            // Trend tăng nhẹ theo thời gian (mô phỏng cửa hàng đang phát triển)
            if (dayOffset < 30) baseOrders += rng.Next(0, 3);
            if (dayOffset < 14) baseOrders += rng.Next(1, 3);

            for (int o = 0; o < baseOrders; o++)
            {
                var customer = allCustomers[rng.Next(allCustomers.Count)];
                var orderTime = date.AddHours(rng.Next(7, 22)).AddMinutes(rng.Next(0, 60));

                // Chọn 1-4 sản phẩm cho đơn hàng
                var itemCount = rng.Next(1, 5);
                var selectedProducts = allProducts.OrderBy(_ => rng.Next()).Take(itemCount).ToList();

                // Tạo mô hình mua sắm: một số khách hàng thích cà phê, một số thích trà
                // Điều này giúp ML recommendation hoạt động tốt hơn
                var customerPreference = customer.Id % 4;
                if (customerPreference == 0 && rng.NextDouble() > 0.3)
                {
                    // Prefer "Cà phê đặc biệt"
                    var preferred = allProducts.Where(p => p.Category == "Cà phê đặc biệt").ToList();
                    if (preferred.Any())
                        selectedProducts[0] = preferred[rng.Next(preferred.Count)];
                }
                else if (customerPreference == 1 && rng.NextDouble() > 0.3)
                {
                    // Prefer "Cà phê đá"
                    var preferred = allProducts.Where(p => p.Category == "Cà phê đá").ToList();
                    if (preferred.Any())
                        selectedProducts[0] = preferred[rng.Next(preferred.Count)];
                }
                else if (customerPreference == 2 && rng.NextDouble() > 0.3)
                {
                    // Prefer "Trà & Khác"
                    var preferred = allProducts.Where(p => p.Category == "Trà & Khác").ToList();
                    if (preferred.Any())
                        selectedProducts[0] = preferred[rng.Next(preferred.Count)];
                }

                var orderItems = selectedProducts.Select(p =>
                {
                    var qty = rng.Next(1, 4);
                    return new OrderItem
                    {
                        ProductId = p.Id,
                        ProductName = p.Name,
                        Quantity = qty,
                        UnitPrice = p.Price
                    };
                }).ToList();

                var total = orderItems.Sum(i => i.Quantity * i.UnitPrice);

                // 85% delivered, 5% pending, 5% ready, 5% cancelled
                var statusRoll = rng.NextDouble();
                var status = statusRoll < 0.85 ? OrderStatus.Delivered
                           : statusRoll < 0.90 ? OrderStatus.Pending
                           : statusRoll < 0.95 ? OrderStatus.Ready
                           : OrderStatus.Cancelled;

                // Đơn gần đây hơn có thể chưa delivered
                if (dayOffset < 2 && status == OrderStatus.Delivered && rng.NextDouble() > 0.5)
                    status = OrderStatus.Pending;

                var paymentMethods = new[] { "COD", "VNPAY", "MoMo" };

                var order = new Order
                {
                    CustomerId = customer.Id,
                    CustomerName = customer.FullName,
                    Status = status,
                    TotalAmount = total,
                    CreatedAt = orderTime,
                    PaymentMethod = paymentMethods[rng.Next(paymentMethods.Length)],
                    IsPaid = status == OrderStatus.Delivered,
                    OrderItems = orderItems
                };

                orders.Add(order);
            }
        }

        context.Orders.AddRange(orders);
        await context.SaveChangesAsync();
    }

    private static async Task UpdateProductImagesAsync(ApplicationDbContext context)
    {
        // Map tên sản phẩm → URL ảnh đúng (fix cho các ảnh bị lỗi)
        var imageMap = new Dictionary<string, string>
        {
            ["Espresso Classico"]  = "https://images.unsplash.com/photo-1510707577719-ae7c14805e3a?w=600&h=400&fit=crop",
            ["Cappuccino Ý"]      = "https://images.unsplash.com/photo-1534778101976-62847782c213?w=600&h=400&fit=crop",
            ["Latte Caramel"]     = "https://images.unsplash.com/photo-1485808191679-5f86510681a2?w=600&h=400&fit=crop",
            ["Mocha Chocolate"]   = "https://images.unsplash.com/photo-1572490122747-3968b75cc699?w=600&h=400&fit=crop",
            ["Affogato"]          = "https://images.unsplash.com/photo-1530373239216-42518e6b4063?w=600&h=400&fit=crop",
            ["Flat White"]        = "https://images.unsplash.com/photo-1611564494260-6f21b80af7ea?w=600&h=400&fit=crop",
            ["Cold Brew Classic"] = "https://images.unsplash.com/photo-1517701550927-30cf4ba1dba5?w=600&h=400&fit=crop",
            ["Bạc Xỉu Đá"]       = "https://images.unsplash.com/photo-1461023058943-07fcbe16d735?w=600&h=400&fit=crop",
            ["Cà Phê Dừa Đá"]    = "https://images.unsplash.com/photo-1509042239860-f550ce710b93?w=600&h=400&fit=crop",
            ["Americano Đá"]      = "https://images.unsplash.com/photo-1551030173-122aabc4489c?w=600&h=400&fit=crop",
            ["Iced Latte Vanilla"]= "https://images.unsplash.com/photo-1592663527359-cf6642f54cff?w=600&h=400&fit=crop",
            ["Cà Phê Sữa Đá"]    = "https://images.unsplash.com/photo-1514432324607-a09d9b4aefda?w=600&h=400&fit=crop",
            ["Cà Phê Phin Nóng"]  = "https://images.unsplash.com/photo-1495474472287-4d71bcdd2085?w=600&h=400&fit=crop",
            ["Cà Phê Trứng"]      = "https://images.unsplash.com/photo-1442512595331-e89e73853f31?w=600&h=400&fit=crop",
            ["Cà Phê Muối"]       = "https://images.unsplash.com/photo-1559496417-e7f25cb247f3?w=600&h=400&fit=crop",
            ["Cà Phê Cốt Dừa Nóng"] = "https://images.unsplash.com/photo-1507133750040-4a8f57021571?w=600&h=400&fit=crop",
            ["Trà Sen Vàng"]      = "https://images.unsplash.com/photo-1564890369478-c89ca6d9cde9?w=600&h=400&fit=crop",
            ["Trà Đào Cam Sả"]    = "https://images.unsplash.com/photo-1556679343-c7306c1976bc?w=600&h=400&fit=crop",
            ["Trà Matcha Latte"]  = "https://images.unsplash.com/photo-1515823064-d6e0c04616a7?w=600&h=400&fit=crop",
            ["Nước Ép Cam Tươi"]  = "https://images.unsplash.com/photo-1621506289937-a8e4df240d0b?w=600&h=400&fit=crop",
            ["Trà Oolong Sữa"]   = "https://images.unsplash.com/photo-1563911892437-1feda0179e1b?w=600&h=400&fit=crop",
            ["Trà Vải Lychee"]    = "https://images.unsplash.com/photo-1556679343-c7306c1976bc?w=600&h=400&fit=crop",
            ["Sinh Tố Bơ"]       = "https://images.unsplash.com/photo-1623065422902-30a2d299bbe4?w=600&h=400&fit=crop",
            ["Socola Nóng"]       = "https://images.unsplash.com/photo-1517578239113-b03992dcdd25?w=600&h=400&fit=crop",
            ["Chanh Dây Đá Xay"]  = "https://images.unsplash.com/photo-1513558161293-cdaf765ed514?w=600&h=400&fit=crop",
            ["Trà Sữa Trân Châu"] = "https://images.unsplash.com/photo-1558857563-b371033873b8?w=600&h=400&fit=crop",
        };

        var products = await context.Products.ToListAsync();
        var updated = false;
        foreach (var product in products)
        {
            if (imageMap.TryGetValue(product.Name, out var correctUrl) && product.ImageUrl != correctUrl)
            {
                product.ImageUrl = correctUrl;
                product.UpdatedAt = DateTime.UtcNow;
                updated = true;
            }
        }
        if (updated) await context.SaveChangesAsync();
    }

    private static List<Product> GetProducts()
    {
        var now = DateTime.UtcNow;
        return new List<Product>
        {
            // ═══ Cà phê đặc biệt ═══
            new Product
            {
                Name = "Espresso Classico",
                Description = "Espresso đậm đặc pha từ hạt Arabica rang vừa, vị đắng thanh hậu ngọt. Chuẩn phong cách Ý.",
                Price = 45000, Category = "Cà phê đặc biệt",
                ImageUrl = "https://images.unsplash.com/photo-1510707577719-ae7c14805e3a?w=600&h=400&fit=crop",
                Stock = 120, IsActive = true, CreatedAt = now, UpdatedAt = now
            },
            new Product
            {
                Name = "Cappuccino Ý",
                Description = "Espresso, sữa nóng tạo bọt mịn, rắc bột cacao — cân bằng hoàn hảo giữa đắng và béo.",
                Price = 55000, Category = "Cà phê đặc biệt",
                ImageUrl = "https://images.unsplash.com/photo-1534778101976-62847782c213?w=600&h=400&fit=crop",
                Stock = 90, IsActive = true, CreatedAt = now, UpdatedAt = now
            },
            new Product
            {
                Name = "Latte Caramel",
                Description = "Espresso + sữa tươi nóng + caramel thơm ngọt, lớp foam mịn phía trên.",
                Price = 60000, Category = "Cà phê đặc biệt",
                ImageUrl = "https://images.unsplash.com/photo-1485808191679-5f86510681a2?w=600&h=400&fit=crop",
                Stock = 80, IsActive = true, CreatedAt = now, UpdatedAt = now
            },
            new Product
            {
                Name = "Mocha Chocolate",
                Description = "Espresso kết hợp chocolate Bỉ đậm và sữa tươi, phủ whipped cream thơm lừng.",
                Price = 65000, Category = "Cà phê đặc biệt",
                ImageUrl = "https://images.unsplash.com/photo-1572490122747-3968b75cc699?w=600&h=400&fit=crop",
                Stock = 70, IsActive = true, CreatedAt = now, UpdatedAt = now
            },
            new Product
            {
                Name = "Affogato",
                Description = "Espresso nóng rót trực tiếp lên kem vanilla gelato — tráng miệng kiểu Ý tuyệt hảo.",
                Price = 70000, Category = "Cà phê đặc biệt",
                ImageUrl = "https://images.unsplash.com/photo-1530373239216-42518e6b4063?w=600&h=400&fit=crop",
                Stock = 50, IsActive = true, CreatedAt = now, UpdatedAt = now
            },
            new Product
            {
                Name = "Flat White",
                Description = "Double ristretto với sữa tươi micro-foam, đậm đà hơn latte nhưng mượt mà hơn cappuccino.",
                Price = 58000, Category = "Cà phê đặc biệt",
                ImageUrl = "https://images.unsplash.com/photo-1611564494260-6f21b80af7ea?w=600&h=400&fit=crop",
                Stock = 65, IsActive = true, CreatedAt = now, UpdatedAt = now
            },

            // ═══ Cà phê đá ═══
            new Product
            {
                Name = "Cold Brew Classic",
                Description = "Cà phê ủ lạnh 18 tiếng, vị mượt không chua, caffeine cao — sảng khoái cả ngày.",
                Price = 50000, Category = "Cà phê đá",
                ImageUrl = "https://images.unsplash.com/photo-1517701550927-30cf4ba1dba5?w=600&h=400&fit=crop",
                Stock = 100, IsActive = true, CreatedAt = now, UpdatedAt = now
            },
            new Product
            {
                Name = "Bạc Xỉu Đá",
                Description = "Cà phê phin pha nhẹ với nhiều sữa đặc + sữa tươi, ngọt dịu lạnh sảng khoái.",
                Price = 35000, Category = "Cà phê đá",
                ImageUrl = "https://images.unsplash.com/photo-1461023058943-07fcbe16d735?w=600&h=400&fit=crop",
                Stock = 150, IsActive = true, CreatedAt = now, UpdatedAt = now
            },
            new Product
            {
                Name = "Cà Phê Dừa Đá",
                Description = "Cà phê đen + kem dừa béo ngậy đánh bông, vị tropical đặc trưng Việt Nam.",
                Price = 45000, Category = "Cà phê đá",
                ImageUrl = "https://images.unsplash.com/photo-1509042239860-f550ce710b93?w=600&h=400&fit=crop",
                Stock = 90, IsActive = true, CreatedAt = now, UpdatedAt = now
            },
            new Product
            {
                Name = "Americano Đá",
                Description = "Espresso pha loãng với nước lạnh và đá viên — đơn giản, thanh khiết, tỉnh táo.",
                Price = 40000, Category = "Cà phê đá",
                ImageUrl = "https://images.unsplash.com/photo-1551030173-122aabc4489c?w=600&h=400&fit=crop",
                Stock = 110, IsActive = true, CreatedAt = now, UpdatedAt = now
            },
            new Product
            {
                Name = "Iced Latte Vanilla",
                Description = "Espresso + sữa tươi lạnh + syrup vanilla Pháp, thơm ngát và mát lạnh.",
                Price = 55000, Category = "Cà phê đá",
                ImageUrl = "https://images.unsplash.com/photo-1592663527359-cf6642f54cff?w=600&h=400&fit=crop",
                Stock = 85, IsActive = true, CreatedAt = now, UpdatedAt = now
            },
            new Product
            {
                Name = "Cà Phê Sữa Đá",
                Description = "Phin Robusta đậm đặc + sữa đặc Ông Thọ + đá viên. Chuẩn vị Sài Gòn!",
                Price = 29000, Category = "Cà phê đá",
                ImageUrl = "https://images.unsplash.com/photo-1514432324607-a09d9b4aefda?w=600&h=400&fit=crop",
                Stock = 200, IsActive = true, CreatedAt = now, UpdatedAt = now
            },

            // ═══ Cà phê truyền thống ═══
            new Product
            {
                Name = "Cà Phê Phin Nóng",
                Description = "Cà phê phin nhôm truyền thống, Robusta Buôn Ma Thuột, nhỏ giọt chậm rãi thơm lừng.",
                Price = 25000, Category = "Cà phê truyền thống",
                ImageUrl = "https://images.unsplash.com/photo-1495474472287-4d71bcdd2085?w=600&h=400&fit=crop",
                Stock = 200, IsActive = true, CreatedAt = now, UpdatedAt = now
            },
            new Product
            {
                Name = "Cà Phê Trứng",
                Description = "Trứng gà đánh bông béo ngậy phủ trên cà phê nóng — đặc sản Hà Nội nổi tiếng thế giới.",
                Price = 45000, Category = "Cà phê truyền thống",
                ImageUrl = "https://images.unsplash.com/photo-1442512595331-e89e73853f31?w=600&h=400&fit=crop",
                Stock = 70, IsActive = true, CreatedAt = now, UpdatedAt = now
            },
            new Product
            {
                Name = "Cà Phê Muối",
                Description = "Cà phê phin đậm + kem muối biển tạo vị mặn-ngọt-đắng hài hòa, trend Huế gây sốt.",
                Price = 42000, Category = "Cà phê truyền thống",
                ImageUrl = "https://images.unsplash.com/photo-1559496417-e7f25cb247f3?w=600&h=400&fit=crop",
                Stock = 80, IsActive = true, CreatedAt = now, UpdatedAt = now
            },
            new Product
            {
                Name = "Cà Phê Cốt Dừa Nóng",
                Description = "Cà phê phin truyền thống kết hợp nước cốt dừa tươi, béo thơm phong cách miền Tây.",
                Price = 38000, Category = "Cà phê truyền thống",
                ImageUrl = "https://images.unsplash.com/photo-1507133750040-4a8f57021571?w=600&h=400&fit=crop",
                Stock = 60, IsActive = true, CreatedAt = now, UpdatedAt = now
            },

            // ═══ Trà & Khác ═══
            new Product
            {
                Name = "Trà Oolong Sữa",
                Description = "Trà Oolong thượng hạng kết hợp sữa tươi, vị chát nhẹ hòa quyện béo ngậy.",
                Price = 48000, Category = "Trà & Khác",
                ImageUrl = "https://images.unsplash.com/photo-1563911892437-1feda0179e1b?w=600&h=400&fit=crop",
                Stock = 75, IsActive = true, CreatedAt = now, UpdatedAt = now
            },
            new Product
            {
                Name = "Trà Vải Lychee",
                Description = "Trà xanh pha vải thiều tươi, thêm thạch dừa — ngọt thanh mát lạnh mùa hè.",
                Price = 45000, Category = "Trà & Khác",
                ImageUrl = "https://images.unsplash.com/photo-1556679343-c7306c1976bc?w=600&h=400&fit=crop",
                Stock = 90, IsActive = true, CreatedAt = now, UpdatedAt = now
            },
            new Product
            {
                Name = "Sinh Tố Bơ",
                Description = "Bơ sáp dầy nguyên chất xay với sữa đặc và đá — béo ngậy, bổ dưỡng.",
                Price = 50000, Category = "Trà & Khác",
                ImageUrl = "https://images.unsplash.com/photo-1623065422902-30a2d299bbe4?w=600&h=400&fit=crop",
                Stock = 60, IsActive = true, CreatedAt = now, UpdatedAt = now
            },
            new Product
            {
                Name = "Socola Nóng",
                Description = "Chocolate Bỉ đun nóng với sữa tươi nguyên kem, phủ marshmallow — ấm áp mùa mưa.",
                Price = 52000, Category = "Trà & Khác",
                ImageUrl = "https://images.unsplash.com/photo-1517578239113-b03992dcdd25?w=600&h=400&fit=crop",
                Stock = 55, IsActive = true, CreatedAt = now, UpdatedAt = now
            },
            new Product
            {
                Name = "Chanh Dây Đá Xay",
                Description = "Chanh dây tươi xay cùng đá mịn, chua ngọt sảng khoái — giải khát số 1 mùa hè.",
                Price = 42000, Category = "Trà & Khác",
                ImageUrl = "https://images.unsplash.com/photo-1513558161293-cdaf765ed514?w=600&h=400&fit=crop",
                Stock = 100, IsActive = true, CreatedAt = now, UpdatedAt = now
            },
            new Product
            {
                Name = "Trà Sữa Trân Châu",
                Description = "Trà đen Ceylon pha sữa tươi, trân châu đen dẻo dai — best seller giới trẻ.",
                Price = 48000, Category = "Trà & Khác",
                ImageUrl = "https://images.unsplash.com/photo-1558857563-b371033873b8?w=600&h=400&fit=crop",
                Stock = 130, IsActive = true, CreatedAt = now, UpdatedAt = now
            },
        };
    }

    private static List<Customer> GetCustomers()
    {
        var now = DateTime.UtcNow;
        return new List<Customer>
        {
            new Customer { FullName = "Nguyễn Văn An",     Phone = "0901000001", CreatedAt = now.AddDays(-90) },
            new Customer { FullName = "Trần Thị Bình",     Phone = "0901000002", CreatedAt = now.AddDays(-85) },
            new Customer { FullName = "Lê Hoàng Cường",    Phone = "0901000003", CreatedAt = now.AddDays(-80) },
            new Customer { FullName = "Phạm Minh Dũng",    Phone = "0901000004", CreatedAt = now.AddDays(-75) },
            new Customer { FullName = "Hoàng Thị Em",      Phone = "0901000005", CreatedAt = now.AddDays(-70) },
            new Customer { FullName = "Vũ Đức Phong",      Phone = "0901000006", CreatedAt = now.AddDays(-60) },
            new Customer { FullName = "Đặng Thị Giang",    Phone = "0901000007", CreatedAt = now.AddDays(-55) },
            new Customer { FullName = "Bùi Quang Huy",     Phone = "0901000008", CreatedAt = now.AddDays(-45) },
            new Customer { FullName = "Ngô Thu Hương",     Phone = "0901000009", CreatedAt = now.AddDays(-30) },
            new Customer { FullName = "Đỗ Minh Khôi",      Phone = "0901000010", CreatedAt = now.AddDays(-20) },
            new Customer { FullName = "Lý Thanh Lam",      Phone = "0901000011", CreatedAt = now.AddDays(-15) },
            new Customer { FullName = "Mai Anh Tuấn",      Phone = "0901000012", CreatedAt = now.AddDays(-10) },
            new Customer { FullName = "Phan Thị Ngọc",     Phone = "0901000013", CreatedAt = now.AddDays(-8) },
            new Customer { FullName = "Tô Văn Phúc",       Phone = "0901000014", CreatedAt = now.AddDays(-5) },
            new Customer { FullName = "Chu Thị Quỳnh",     Phone = "0901000015", CreatedAt = now.AddDays(-3) },
        };
    }
}
