# 🔧 Integration Examples

## 1️⃣ Sử dụng Navbar trong mọi trang

### Tạo/Cập nhật `Views/Shared/_Layout.cshtml`

```html
@{
    Layout = null;
}

<!DOCTYPE html>
<html lang="vi">
<head>
    <meta charset="UTF-8">
    <meta name="viewport" content="width=device-width, initial-scale=1.0">
    <title>@ViewData["Title"] - Coffee Shop</title>
    <script src="https://cdn.tailwindcss.com"></script>
    <link href="https://fonts.googleapis.com/css2?family=Playfair+Display:wght@700&family=Inter:wght@400;500;600;700&display=swap" rel="stylesheet">
    <style>
        body { font-family: 'Inter', sans-serif; }
        h1, h2, h3 { font-family: 'Playfair Display', serif; }
    </style>
</head>
<body class="bg-[#FAF8F5]">
    <!-- 🔝 Navbar Component -->
    @await Component.InvokeAsync("Navbar")

    <!-- Main Content -->
    <main>
        @RenderBody()
    </main>

    <!-- Footer -->
    <footer class="bg-[#2C1810] text-white py-8 mt-12">
        <div class="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
            <div class="grid grid-cols-3 gap-8 mb-8">
                <div>
                    <h4 class="font-bold mb-4">Về Chúng Tôi</h4>
                    <p class="text-sm text-gray-300">Cửa hàng cà phê premium tại TP.HCM</p>
                </div>
                <div>
                    <h4 class="font-bold mb-4">Liên Kết</h4>
                    <ul class="space-y-2 text-sm">
                        <li><a href="/" class="hover:text-[#D4A574]">Trang Chủ</a></li>
                        <li><a href="/Pages/About" class="hover:text-[#D4A574]">Giới Thiệu</a></li>
                        <li><a href="/Pages/Contact" class="hover:text-[#D4A574]">Liên Hệ</a></li>
                    </ul>
                </div>
                <div>
                    <h4 class="font-bold mb-4">Liên Hệ</h4>
                    <p class="text-sm text-gray-300">📞 (028) 1234 5678</p>
                    <p class="text-sm text-gray-300">📧 hello@coffeeshop.vn</p>
                </div>
            </div>
            <div class="border-t border-gray-700 pt-6 text-center text-gray-400 text-sm">
                <p>&copy; 2024 Cửa Hàng Cà Phê. All rights reserved.</p>
            </div>
        </div>
    </footer>

    @await RenderSectionAsync("Scripts", required: false)
</body>
</html>
```

### Cập nhật `Views/_ViewStart.cshtml`

```html
@{
    Layout = "_Layout";
}
```

---

## 2️⃣ Sử dụng BestSellingProducts trong Home/Index

### Cập nhật `Views/Home/Index.cshtml`

```html
@{
    ViewData["Title"] = "Trang Chủ - Cửa Hàng Cà Phê";
    Layout = "_Layout"; // Sử dụng default layout với Navbar
}

<!-- Hero Section -->
<section class="bg-gradient-to-r from-[#6F4E37] to-[#2C1810] text-white py-20">
    <div class="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 text-center">
        <h1 class="text-5xl font-bold mb-4">Cà Phê Tươi Mỗi Ngày</h1>
        <p class="text-xl text-gray-300 mb-8">Chất lượng cao, hương vị thơm ngon, phục vụ nhiệt tình</p>
        <a href="/Pages/About" class="inline-block bg-[#D4A574] text-white px-8 py-3 rounded-lg font-semibold hover:bg-[#C89968] transition">
            Tìm Hiểu Thêm
        </a>
    </div>
</section>

<!-- 🌟 Best Selling Products Component -->
@await Component.InvokeAsync("BestSellingProducts")

<!-- Features Section -->
<section class="py-16 bg-white">
    <div class="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
        <h2 class="text-3xl font-bold text-center text-[#2C1810] mb-12">Tại Sao Chọn Chúng Tôi?</h2>
        <div class="grid grid-cols-3 gap-8">
            <div class="text-center">
                <div class="text-5xl mb-4">☕</div>
                <h3 class="text-xl font-bold text-[#2C1810] mb-2">Cà Phê Cao Cấp</h3>
                <p class="text-gray-600">Lựa chọn từ những vùng trồng cà phê tốt nhất thế giới</p>
            </div>
            <div class="text-center">
                <div class="text-5xl mb-4">🎯</div>
                <h3 class="text-xl font-bold text-[#2C1810] mb-2">Rang Xay Tươi</h3>
                <p class="text-gray-600">Rang xay tươi hàng ngày để giữ hương vị tuyệt hảo</p>
            </div>
            <div class="text-center">
                <div class="text-5xl mb-4">🤝</div>
                <h3 class="text-xl font-bold text-[#2C1810] mb-2">Dịch Vụ Tốt</h3>
                <p class="text-gray-600">Đội ngũ nhân viên tận tâm phục vụ từ tâm</p>
            </div>
        </div>
    </div>
</section>
```

---

## 3️⃣ Sử dụng StatusBadge trong Admin Order views

### Cập nhật `Areas/Admin/Views/Order/Index.cshtml`

```html
@model IEnumerable<SaleStore.Models.Order>

@{
    ViewData["Title"] = "Quản Lý Đơn Hàng";
}

<div class="container-fluid mt-4">
    <div class="row mb-4">
        <div class="col-md-6">
            <h2>Danh Sách Đơn Hàng</h2>
        </div>
        <div class="col-md-6 text-end">
            <span class="text-muted">Tổng cộng: @Model.Count() đơn hàng</span>
        </div>
    </div>

    <div class="table-responsive">
        <table class="table table-hover">
            <thead class="table-light">
                <tr>
                    <th>Mã Đơn</th>
                    <th>Khách Hàng</th>
                    <th>Ngày Đặt</th>
                    <th>Tổng Tiền</th>
                    <th>Trạng Thái</th>
                    <th>Thao Tác</th>
                </tr>
            </thead>
            <tbody>
                @foreach (var order in Model)
                {
                    <tr>
                        <td><strong>#@order.Id</strong></td>
                        <td>@order.CustomerName</td>
                        <td>@order.CreatedAt.ToString("dd/MM/yyyy HH:mm")</td>
                        <td>@order.TotalPrice.ToString("C0")</td>
                        <td>
                            <!-- 🎯 Status Badge Component -->
                            @await Component.InvokeAsync("StatusBadge", new { status = order.Status })
                        </td>
                        <td>
                            <a href="/Admin/Order/Detail/@order.Id" class="btn btn-sm btn-primary">Xem Chi Tiết</a>
                        </td>
                    </tr>
                }
            </tbody>
        </table>
    </div>
</div>
```

### Cập nhật `Areas/Admin/Views/Order/Detail.cshtml`

```html
@model SaleStore.Models.Order

@{
    ViewData["Title"] = "Chi Tiết Đơn Hàng";
}

<div class="container-fluid mt-4">
    <div class="row mb-4">
        <div class="col-md-8">
            <h2>Đơn Hàng #@Model.Id</h2>
        </div>
        <div class="col-md-4">
            <!-- 🎯 Status Badge with larger size -->
            <div style="font-size: 1.2rem;">
                @await Component.InvokeAsync("StatusBadge", new { status = Model.Status })
            </div>
        </div>
    </div>

    <div class="row">
        <div class="col-md-8">
            <div class="card">
                <div class="card-header">
                    <h5>Thông Tin Đơn Hàng</h5>
                </div>
                <div class="card-body">
                    <p><strong>Khách Hàng:</strong> @Model.CustomerName</p>
                    <p><strong>Email:</strong> @Model.CustomerEmail</p>
                    <p><strong>Điện Thoại:</strong> @Model.CustomerPhone</p>
                    <p><strong>Địa Chỉ:</strong> @Model.DeliveryAddress</p>
                    <p><strong>Ngày Đặt:</strong> @Model.CreatedAt.ToString("dd/MM/yyyy HH:mm")</p>
                </div>
            </div>

            <div class="card mt-3">
                <div class="card-header">
                    <h5>Chi Tiết Sản Phẩm</h5>
                </div>
                <div class="card-body">
                    <table class="table">
                        <thead>
                            <tr>
                                <th>Sản Phẩm</th>
                                <th>Giá</th>
                                <th>Số Lượng</th>
                                <th>Tổng</th>
                            </tr>
                        </thead>
                        <tbody>
                            @foreach (var item in Model.OrderItems)
                            {
                                <tr>
                                    <td>@item.ProductName</td>
                                    <td>@item.Price.ToString("C0")</td>
                                    <td>@item.Quantity</td>
                                    <td>@(item.Price * item.Quantity).ToString("C0")</td>
                                </tr>
                            }
                        </tbody>
                    </table>
                </div>
            </div>
        </div>

        <div class="col-md-4">
            <div class="card">
                <div class="card-header">
                    <h5>Tóm Tắt</h5>
                </div>
                <div class="card-body">
                    <div class="d-flex justify-content-between mb-2">
                        <span>Tổng Tiền:</span>
                        <strong>@Model.TotalPrice.ToString("C0")</strong>
                    </div>
                    <hr />
                    <div class="mb-3">
                        <label>Cập Nhật Trạng Thái:</label>
                        <form method="post" action="/Admin/Order/UpdateStatus">
                            <select name="status" class="form-control mb-2">
                                @foreach (var s in Enum.GetValues(typeof(SaleStore.Models.OrderStatus)))
                                {
                                    <option value="@s" selected="@(s.Equals(Model.Status))">@s</option>
                                }
                            </select>
                            <input type="hidden" name="orderId" value="@Model.Id" />
                            <button type="submit" class="btn btn-primary w-100">Cập Nhật</button>
                        </form>
                    </div>
                </div>
            </div>
        </div>
    </div>
</div>
```

---

## 4️⃣ Sử dụng UserAvatar trong Admin Layout

### Cập nhật `Areas/Admin/Views/Shared/_AdminLayout.cshtml`

```html
@{
    Layout = null;
}

<!DOCTYPE html>
<html lang="vi">
<head>
    <meta charset="UTF-8">
    <meta name="viewport" content="width=device-width, initial-scale=1.0">
    <title>@ViewData["Title"] - Admin Panel</title>
    <link href="~/lib/bootstrap/css/bootstrap.min.css" rel="stylesheet" />
    <style>
        .sidebar { background-color: #2C1810; color: white; }
        .sidebar a { color: #D4A574; text-decoration: none; }
        .navbar { background-color: #f8f9fa; }
    </style>
</head>
<body>
    <!-- Header/Navbar -->
    <nav class="navbar navbar-expand-lg navbar-light">
        <div class="container-fluid">
            <span class="navbar-brand">Admin Panel</span>
            <div class="ms-auto">
                <div class="d-flex align-items-center gap-3">
                    <!-- 👤 User Avatar Component -->
                    <div style="width: 40px;">
                        @await Component.InvokeAsync("UserAvatar", new { cssClass = "w-10 h-10" })
                    </div>
                    <span>@User.Identity?.Name</span>
                    <form method="post" action="/Auth/Logout">
                        <button type="submit" class="btn btn-danger btn-sm">Đăng Xuất</button>
                    </form>
                </div>
            </div>
        </div>
    </nav>

    <div class="container-fluid">
        <div class="row">
            <!-- Sidebar -->
            <div class="col-md-3 sidebar p-3">
                <ul class="list-unstyled">
                    <li class="mb-3"><a href="/Admin">Dashboard</a></li>
                    <li class="mb-3"><a href="/Admin/Product">Quản Lý Sản Phẩm</a></li>
                    <li class="mb-3"><a href="/Admin/Order">Quản Lý Đơn Hàng</a></li>
                    <li class="mb-3"><a href="/Admin/UserManagement">Quản Lý Nhân Viên</a></li>
                </ul>

                <hr class="border-secondary" />

                <!-- User Info with Avatar -->
                <div class="d-flex align-items-center gap-2">
                    <div style="width: 50px;">
                        @await Component.InvokeAsync("UserAvatar", new { cssClass = "w-12 h-12" })
                    </div>
                    <div>
                        <small class="text-muted">Đăng nhập tại:</small>
                        <p class="mb-0">@User.Identity?.Name</p>
                    </div>
                </div>
            </div>

            <!-- Main Content -->
            <div class="col-md-9 p-4">
                @RenderBody()
            </div>
        </div>
    </div>

    <script src="~/lib/bootstrap/js/bootstrap.bundle.min.js"></script>
</body>
</html>
```

---

## 5️⃣ Sử dụng tất cả Components trong Home Page

### `Views/Home/Index.cshtml` (Complete Example)

```html
@{
    ViewData["Title"] = "Trang Chủ - Cửa Hàng Cà Phê";
    Layout = "_Layout";
}

<!-- Hero Section -->
<section class="py-20 px-4 bg-gradient-to-r from-[#6F4E37] to-[#2C1810]">
    <div class="max-w-7xl mx-auto text-center text-white">
        <h1 class="text-5xl font-bold mb-4">☕ Cà Phê Tươi Mỗi Ngày</h1>
        <p class="text-2xl mb-8 text-gray-300">
            Hương vị cao cấp, chất lượng đảm bảo, phục vụ từ tâm
        </p>
        <div class="space-x-4">
            <a href="/Cart/Checkout" class="inline-block px-8 py-3 bg-[#D4A574] hover:bg-[#C89968] rounded-lg font-semibold transition">
                Đặt Hàng Ngay
            </a>
            <a href="/Pages/About" class="inline-block px-8 py-3 border-2 border-white hover:bg-white hover:text-[#2C1810] rounded-lg font-semibold transition">
                Giới Thiệu
            </a>
        </div>
    </div>
</section>

<!-- Best Selling Products -->
@await Component.InvokeAsync("BestSellingProducts")

<!-- Why Choose Us -->
<section class="py-16 bg-white">
    <div class="max-w-7xl mx-auto px-4">
        <h2 class="text-4xl font-bold text-center text-[#2C1810] mb-12">
            Tại Sao Chọn Chúng Tôi?
        </h2>
        <div class="grid grid-cols-1 md:grid-cols-3 gap-8">
            <div class="text-center p-6 bg-[#FAF8F5] rounded-lg">
                <span class="text-5xl block mb-4">☕</span>
                <h3 class="text-2xl font-bold text-[#2C1810] mb-2">Cà Phê 100% Nguyên Chất</h3>
                <p class="text-gray-600">
                    Không pha trộn, không gia vị nhân tạo, 100% cà phê nguyên chất
                </p>
            </div>

            <div class="text-center p-6 bg-[#FAF8F5] rounded-lg">
                <span class="text-5xl block mb-4">🎯</span>
                <h3 class="text-2xl font-bold text-[#2C1810] mb-2">Rang Xay Tươi</h3>
                <p class="text-gray-600">
                    Rang xay tươi mỗi ngày để giữ hương vị tươi ngon nhất
                </p>
            </div>

            <div class="text-center p-6 bg-[#FAF8F5] rounded-lg">
                <span class="text-5xl block mb-4">🤝</span>
                <h3 class="text-2xl font-bold text-[#2C1810] mb-2">Dịch Vụ Tuyệt Vời</h3>
                <p class="text-gray-600">
                    Đội ngũ nhân viên chuyên nghiệp phục vụ với tâm tình
                </p>
            </div>
        </div>
    </div>
</section>

<!-- Testimonials / Reviews -->
<section class="py-16 bg-[#FAF8F5]">
    <div class="max-w-7xl mx-auto px-4">
        <h2 class="text-4xl font-bold text-center text-[#2C1810] mb-12">
            Khách Hàng Nói Gì Về Chúng Tôi?
        </h2>
        <div class="grid grid-cols-1 md:grid-cols-3 gap-8">
            <div class="bg-white p-6 rounded-lg shadow-md">
                <div class="flex gap-2 mb-4">
                    <span>⭐⭐⭐⭐⭐</span>
                </div>
                <p class="text-gray-700 mb-4">
                    "Cà phê rất ngon, và đội ngũ phục vụ rất nhiệt tình. Tôi sẽ quay lại!"
                </p>
                <p class="font-bold text-[#2C1810]">- Nguyễn Văn A</p>
            </div>

            <div class="bg-white p-6 rounded-lg shadow-md">
                <div class="flex gap-2 mb-4">
                    <span>⭐⭐⭐⭐⭐</span>
                </div>
                <p class="text-gray-700 mb-4">
                    "Chất lượng cà phê cao nhất trong khu vực. Giá cả hợp lý, không lo bị chặt chém."
                </p>
                <p class="font-bold text-[#2C1810]">- Trần Thị B</p>
            </div>

            <div class="bg-white p-6 rounded-lg shadow-md">
                <div class="flex gap-2 mb-4">
                    <span>⭐⭐⭐⭐⭐</span>
                </div>
                <p class="text-gray-700 mb-4">
                    "Tôi đã ghé thăm nhiều lần, mỗi lần đều có cảm giác như ở nhà."
                </p>
                <p class="font-bold text-[#2C1810]">- Lê Văn C</p>
            </div>
        </div>
    </div>
</section>

<!-- CTA Section -->
<section class="py-16 bg-[#2C1810] text-white">
    <div class="max-w-4xl mx-auto text-center px-4">
        <h2 class="text-4xl font-bold mb-4">Sẵn Sàng Thưởng Thức Cà Phê Tuyệt Hảo?</h2>
        <p class="text-xl mb-8 text-gray-300">
            Ghé thăm cửa hàng của chúng tôi hoặc liên hệ để biết thêm chi tiết
        </p>
        <div class="space-x-4">
            <a href="/Pages/Contact" class="inline-block px-8 py-3 bg-[#D4A574] hover:bg-[#C89968] rounded-lg font-semibold transition">
                Liên Hệ Ngay
            </a>
            <a href="/" class="inline-block px-8 py-3 border-2 border-[#D4A574] text-[#D4A574] hover:bg-[#D4A574] hover:text-white rounded-lg font-semibold transition">
                Xem Sản Phẩm
            </a>
        </div>
    </div>
</section>
```

---

## 📝 Checklist Hoàn Thành

- [ ] Cập nhật `Program.cs` với `AddRazorPages()` và `MapRazorPages()`
- [ ] Tạo `Views/Shared/_Layout.cshtml` với Navbar component
- [ ] Cập nhật `Views/_ViewStart.cshtml`
- [ ] Cập nhật `Views/Home/Index.cshtml` với BestSellingProducts
- [ ] Cập nhật Admin Order views với StatusBadge
- [ ] Cập nhật Admin Layout với UserAvatar
- [ ] Test tất cả các Razor Pages (`/Pages/About`, `/Pages/Contact`)
- [ ] Test form Contact page
- [ ] Kiểm tra responsive design trên mobile

---

**Good Luck! ☕**
