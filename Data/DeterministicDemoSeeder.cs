using System.Globalization;
using Microsoft.EntityFrameworkCore;
using SaleStore.Models;

namespace SaleStore.Data;

public static class DeterministicDemoSeeder
{
    private const string OrderMarkerPrefix = "COFFEE-DEMO-V1:";
    private static readonly DateTime DefaultAnchorDate = new(2026, 9, 1, 0, 0, 0, DateTimeKind.Utc);

    public static async Task SeedAsync(ApplicationDbContext context, IConfiguration configuration)
    {
        var anchorDate = ParseAnchorDate(configuration["DemoSeed:AnchorDate"]);
        await EnsureProductsAsync(context, anchorDate);

        var products = await context.Products
            .Where(x => x.IsActive && x.Price > 0)
            .OrderBy(x => x.Name)
            .Take(20)
            .ToListAsync();

        if (products.Count < 4)
            throw new InvalidOperationException("Demo seed requires at least four active products.");

        var customers = await EnsureCustomersAsync(context, anchorDate);
        await EnsureHistoricalOrdersAsync(context, products, customers, anchorDate);
    }

    private static DateTime ParseAnchorDate(string? configuredValue)
    {
        if (string.IsNullOrWhiteSpace(configuredValue))
            return DefaultAnchorDate;

        if (!DateTime.TryParseExact(
                configuredValue,
                "yyyy-MM-dd",
                CultureInfo.InvariantCulture,
                DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal,
                out var parsed))
        {
            throw new InvalidOperationException("DemoSeed:AnchorDate must use yyyy-MM-dd format.");
        }

        return DateTime.SpecifyKind(parsed.Date, DateTimeKind.Utc);
    }

    private static async Task EnsureProductsAsync(ApplicationDbContext context, DateTime anchorDate)
    {
        if (await context.Products.CountAsync(x => x.IsActive) >= 12)
            return;

        var products = new[]
        {
            Product("Demo Espresso", "Classic balanced espresso.", 35000, "Coffee", 120, "https://images.unsplash.com/photo-1510707577719-ae7c14805e3a?w=600&h=400&fit=crop", anchorDate),
            Product("Demo Americano", "Espresso lengthened with hot water.", 40000, "Coffee", 110, "https://images.unsplash.com/photo-1551030173-122aabc4489c?w=600&h=400&fit=crop", anchorDate),
            Product("Demo Cold Brew", "Smooth slow-steeped cold coffee.", 50000, "Coffee", 90, "https://images.unsplash.com/photo-1517701550927-30cf4ba1dba5?w=600&h=400&fit=crop", anchorDate),
            Product("Demo Milk Coffee", "Vietnamese coffee with condensed milk.", 39000, "Milk Coffee", 140, "https://images.unsplash.com/photo-1461023058943-07fcbe16d735?w=600&h=400&fit=crop", anchorDate),
            Product("Demo Cafe Latte", "Espresso with steamed fresh milk.", 55000, "Milk Coffee", 100, "https://images.unsplash.com/photo-1485808191679-5f86510681a2?w=600&h=400&fit=crop", anchorDate),
            Product("Demo Cappuccino", "Espresso, steamed milk, and fine foam.", 55000, "Milk Coffee", 95, "https://images.unsplash.com/photo-1534778101976-62847782c213?w=600&h=400&fit=crop", anchorDate),
            Product("Demo Lotus Tea", "Fragrant lotus tea served chilled.", 45000, "Tea", 100, "https://images.unsplash.com/photo-1564890369478-c89ca6d9cde9?w=600&h=400&fit=crop", anchorDate),
            Product("Demo Peach Tea", "Black tea with peach and citrus.", 48000, "Tea", 90, "https://images.unsplash.com/photo-1556679343-c7306c1976bc?w=600&h=400&fit=crop", anchorDate),
            Product("Demo Matcha Latte", "Matcha blended with fresh milk.", 56000, "Tea", 80, "https://images.unsplash.com/photo-1515823064-d6e0c04616a7?w=600&h=400&fit=crop", anchorDate),
            Product("Demo Orange Juice", "Fresh orange juice without additives.", 42000, "Other Beverages", 70, "https://images.unsplash.com/photo-1621506289937-a8e4df240d0b?w=600&h=400&fit=crop", anchorDate),
            Product("Demo Hot Chocolate", "Rich cocoa with steamed milk.", 52000, "Other Beverages", 65, "https://images.unsplash.com/photo-1517578239113-b03992dcdd25?w=600&h=400&fit=crop", anchorDate),
            Product("Demo Passion Fruit Ice", "Bright passion fruit over crushed ice.", 45000, "Other Beverages", 75, "https://images.unsplash.com/photo-1513558161293-cdaf765ed514?w=600&h=400&fit=crop", anchorDate)
        };

        var existingNames = await context.Products.Select(x => x.Name).ToHashSetAsync();
        context.Products.AddRange(products.Where(x => !existingNames.Contains(x.Name)));
        await context.SaveChangesAsync();
    }

    private static Product Product(
        string name,
        string description,
        decimal price,
        string category,
        int stock,
        string imageUrl,
        DateTime createdAt) => new()
    {
        Name = name,
        Description = description,
        Price = price,
        Category = category,
        Stock = stock,
        ImageUrl = imageUrl,
        IsActive = true,
        CreatedAt = createdAt,
        UpdatedAt = createdAt
    };

    private static async Task<List<Customer>> EnsureCustomersAsync(
        ApplicationDbContext context,
        DateTime anchorDate)
    {
        var definitions = Enumerable.Range(1, 10)
            .Select(index => new
            {
                Phone = $"090900{index:0000}",
                Name = $"CoffeeShop Demo Customer {index:00}"
            })
            .ToList();

        var phones = definitions.Select(x => x.Phone).ToList();
        var existing = await context.Customers
            .Where(x => phones.Contains(x.Phone))
            .ToListAsync();
        var existingPhones = existing.Select(x => x.Phone).ToHashSet();

        context.Customers.AddRange(definitions
            .Where(x => !existingPhones.Contains(x.Phone))
            .Select((x, index) => new Customer
            {
                FullName = x.Name,
                Phone = x.Phone,
                CreatedAt = anchorDate.AddDays(-120 + index)
            }));
        await context.SaveChangesAsync();

        return await context.Customers
            .Where(x => phones.Contains(x.Phone))
            .OrderBy(x => x.Phone)
            .ToListAsync();
    }

    private static async Task EnsureHistoricalOrdersAsync(
        ApplicationDbContext context,
        IReadOnlyList<Product> products,
        IReadOnlyList<Customer> customers,
        DateTime anchorDate)
    {
        var existingMarkers = await context.Orders
            .Where(x => x.Note != null && x.Note.StartsWith(OrderMarkerPrefix))
            .Select(x => x.Note!)
            .ToHashSetAsync();

        var orders = new List<Order>();
        for (var day = 0; day < 90; day++)
        {
            var marker = $"{OrderMarkerPrefix}{day:000}";
            if (existingMarkers.Contains(marker))
                continue;

            var customer = customers[day % customers.Count];
            var itemCount = 2 + day % 3;
            var items = Enumerable.Range(0, itemCount)
                .Select(offset =>
                {
                    var product = products[(day * 3 + offset * 5) % products.Count];
                    return new OrderItem
                    {
                        ProductId = product.Id,
                        ProductName = product.Name,
                        Quantity = 1 + (day + offset) % 3,
                        UnitPrice = product.Price
                    };
                })
                .ToList();

            var age = 89 - day;
            var status = age switch
            {
                0 => OrderStatus.Pending,
                1 => OrderStatus.Ready,
                2 => OrderStatus.Cancelled,
                _ => OrderStatus.Delivered
            };
            var total = items.Sum(x => x.Quantity * x.UnitPrice);

            orders.Add(new Order
            {
                CustomerId = customer.Id,
                CustomerName = customer.FullName,
                ShippingAddress = "CoffeeShop demo delivery zone",
                Note = marker,
                Status = status,
                TotalAmount = total,
                CreatedAt = anchorDate.AddDays(-age).AddHours(8 + day % 12),
                PaymentMethod = "COD",
                IsPaid = status == OrderStatus.Delivered,
                OrderItems = items
            });
        }

        if (orders.Count == 0)
            return;

        context.Orders.AddRange(orders);
        await context.SaveChangesAsync();
    }
}
