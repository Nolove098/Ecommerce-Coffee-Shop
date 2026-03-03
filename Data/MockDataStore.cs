using SaleStore.Models;

namespace SaleStore.Data
{
    /// <summary>
    /// In-memory data store thay thế DbContext cho mục đích demo.
    /// Dữ liệu sẽ reset mỗi khi app khởi động lại.
    /// </summary>
    public static class MockDataStore
    {
        private static long _productIdCounter = 7;
        private static long _orderIdCounter = 6;

        public static List<Product> Products { get; } = new()
        {
            new Product { ProductID = 1, Name = "Caramel Macchiato", Description = "Espresso với sữa hấp và caramel thơm ngon", Price = 65000, Category = "Cà phê đặc biệt", ImageUrl = "https://images.unsplash.com/photo-1572442388796-11668a67e53d?w=400", Stock = 50, IsActive = true },
            new Product { ProductID = 2, Name = "Cappuccino", Description = "Espresso kết hợp sữa tươi và bọt sữa mịn", Price = 55000, Category = "Cà phê đặc biệt", ImageUrl = "https://images.unsplash.com/photo-1606791405792-1004f1718d0c?w=400", Stock = 60, IsActive = true },
            new Product { ProductID = 3, Name = "Iced Latte", Description = "Cà phê latte đá lạnh mát mẻ", Price = 55000, Category = "Cà phê đá", ImageUrl = "https://images.unsplash.com/photo-1517487881594-2787fef5ebf7?w=400", Stock = 80, IsActive = true },
            new Product { ProductID = 4, Name = "Espresso", Description = "Cà phê espresso đậm đà nguyên chất", Price = 40000, Category = "Cà phê truyền thống", ImageUrl = "https://images.unsplash.com/photo-1579992357154-faf4bde95b3d?w=400", Stock = 100, IsActive = true },
            new Product { ProductID = 5, Name = "Mocha", Description = "Espresso kết hợp với sô-cô-la và sữa", Price = 60000, Category = "Cà phê đặc biệt", ImageUrl = "https://images.unsplash.com/photo-1607681034540-2c46cc71896d?w=400", Stock = 45, IsActive = true },
            new Product { ProductID = 6, Name = "Matcha Latte", Description = "Matcha Nhật Bản hòa cùng sữa tươi béo ngậy", Price = 65000, Category = "Trà & Khác", ImageUrl = "https://images.unsplash.com/photo-1536256263959-770b48d82b0a?w=400", Stock = 30, IsActive = false },
        };

        public static List<Order> Orders { get; } = new()
        {
            new Order { OrderID = 1, CustomerName = "Nguyễn Văn An", CustomerPhone = "0901234567", Status = OrderStatus.Delivered, CreatedAt = DateTime.Now.AddHours(-3), Items = new() { new OrderItem { ProductID = 1, ProductName = "Caramel Macchiato", Quantity = 2, UnitPrice = 65000 }, new OrderItem { ProductID = 4, ProductName = "Espresso", Quantity = 1, UnitPrice = 40000 } } },
            new Order { OrderID = 2, CustomerName = "Trần Thị Bích", CustomerPhone = "0912345678", Status = OrderStatus.Brewing, CreatedAt = DateTime.Now.AddMinutes(-30), Items = new() { new OrderItem { ProductID = 2, ProductName = "Cappuccino", Quantity = 1, UnitPrice = 55000 } } },
            new Order { OrderID = 3, CustomerName = "Lê Hữu Phúc", CustomerPhone = "0923456789", Status = OrderStatus.Pending, CreatedAt = DateTime.Now.AddMinutes(-5), Items = new() { new OrderItem { ProductID = 3, ProductName = "Iced Latte", Quantity = 2, UnitPrice = 55000 }, new OrderItem { ProductID = 5, ProductName = "Mocha", Quantity = 1, UnitPrice = 60000 } } },
            new Order { OrderID = 4, CustomerName = "Phạm Thùy Linh", CustomerPhone = "0934567890", Status = OrderStatus.Ready, CreatedAt = DateTime.Now.AddMinutes(-15), Items = new() { new OrderItem { ProductID = 6, ProductName = "Matcha Latte", Quantity = 2, UnitPrice = 65000 } } },
            new Order { OrderID = 5, CustomerName = "Hoàng Minh Tuấn", CustomerPhone = "0945678901", Status = OrderStatus.Cancelled, CreatedAt = DateTime.Now.AddHours(-1), Note = "Khách hủy vì đợi lâu", Items = new() { new OrderItem { ProductID = 1, ProductName = "Caramel Macchiato", Quantity = 3, UnitPrice = 65000 } } },
        };

        public static long NextProductId() => ++_productIdCounter;
        public static long NextOrderId() => ++_orderIdCounter;
    }
}
