# 📚 Hướng dẫn View Components & Razor Pages

## 📋 Mục lục
1. [View Components](#view-components)
2. [Razor Pages](#razor-pages)
3. [Cách sử dụng](#cách-sử-dụng)
4. [Ví dụ thực tế](#ví-dụ-thực-tế)
5. [Best Practices](#best-practices)

---

## 🎨 View Components

View Components là các UI logic độc lập, có thể tái sử dụng được. Chúng tương tự như Partial Views nhưng có thêm C# code-behind.

### ✅ View Components được tạo:

#### 1. **NavbarViewComponent** 📱
**Vị trí:** `Views/Components/Navbar/`
- **C# Class:** `NavbarViewComponent.cs`
- **View:** `Default.cshtml`

**Chức năng:**
- Hiển thị thanh navigation chung cho toàn bộ ứng dụng
- Tự động hiển thị thông tin user nếu đã đăng nhập
- Hiển thị dropdown menu với tùy chọn Đăng xuất
- Responsive design cho mobile

**Sử dụng trong view:**
```html
@await Component.InvokeAsync("Navbar")
```

---

#### 2. **BestSellingProductsViewComponent** ☕
**Vị trí:** `Views/Components/BestSellingProducts/`
- **C# Class:** `BestSellingProductsViewComponent.cs`
- **View:** `Default.cshtml`

**Chức năng:**
- Lấy 6 sản phẩm bán chạy nhất từ database
- Hiển thị hình ảnh, giá, mô tả, và trạng thái hàng
- Nút "Thêm vào giỏ hàng" có thể tùy chỉnh
- Hiển thị badge "Hết hàng" hoặc "Sắp hết"

**Sử dụng trong view:**
```html
@await Component.InvokeAsync("BestSellingProducts")
```

**Yêu cầu:** 
- DbContext phải có quyền truy cập Products table

---

#### 3. **StatusBadgeViewComponent** 🏷️
**Vị trị:** `Views/Components/StatusBadge/`
- **C# Class:** `StatusBadgeViewComponent.cs`
- **View:** `Default.cshtml`

**Chức năng:**
- Hiển thị status badge với màu sắc khác nhau
- Hỗ trợ các trạng thái:
  - `Pending` - Chờ (Vàng)
  - `Brewing` - Đang pha (Xanh dương)
  - `Ready` - Sẵn sàng (Xanh lá)
  - `Delivered` - Đã giao (Tím)
  - `Cancelled` - Đã hủy (Đỏ)

**Sử dụng trong view:**
```html
@await Component.InvokeAsync("StatusBadge", new { status = order.Status })
```

**Ví dụ trong Order Index:**
```html
<!-- Trước (cách cũ) -->
<span class="badge-pending">Chờ xử lý</span>

<!-- Sau (sử dụng component) -->
@await Component.InvokeAsync("StatusBadge", new { status = order.Status })
```

---

#### 4. **UserAvatarViewComponent** 👤
**Vị trị:** `Views/Components/UserAvatar/`
- **C# Class:** `UserAvatarViewComponent.cs` (kèm ViewModel)
- **View:** `Default.cshtml`

**Chức năng:**
- Hiển thị avatar hình tròn với chữ cái đầu tiên của username
- Có thể tùy chỉnh kích thước qua parameter `cssClass`
- Hiển thị tên user khi hover chuột

**Sử dụng trong view:**
```html
<!-- Kích thước mặc định (10x10) -->
@await Component.InvokeAsync("UserAvatar")

<!-- Kích thước lớn (16x16) -->
@await Component.InvokeAsync("UserAvatar", new { cssClass = "w-16 h-16" })

<!-- Với username khác -->
@await Component.InvokeAsync("UserAvatar", new { userName = "John Doe" })
```

---

## 📄 Razor Pages

Razor Pages là một cách đơn giản và sạch sẽ để xây dựng các trang web độc lập trong ASP.NET Core.

### ✅ Razor Pages được tạo:

#### 1. **About Page** ℹ️
**Vị trị:** `Pages/About.cshtml` và `Pages/About.cshtml.cs`
**URL:** `/Pages/About`

**Chức năng:**
- Trang thông tin về cửa hàng cà phê
- Hiển thị lịch sử, sứ mệnh, tầm nhìn
- Các giá trị cốt lõi của công ty
- Call-to-action link đến trang Contact

**Cấu trúc:**
```csharp
public class AboutModel : PageModel
{
    public void OnGet()
    {
        ViewData["Title"] = "Giới thiệu - Cửa hàng Cà phê";
    }
}
```

---

#### 2. **Contact Page** 📧
**Vị trị:** `Pages/Contact.cshtml` và `Pages/Contact.cshtml.cs`
**URL:** `/Pages/Contact`

**Chức năng:**
- Trang liên hệ với form gửi tin nhắn
- Hiển thị thông tin liên hệ (địa chỉ, điện thoại, email)
- Form xử lý POST request
- Hiển thị thông báo thành công sau khi gửi

**Cấu trúc PageModel:**
```csharp
public class ContactModel : PageModel
{
    [BindProperty]
    public ContactForm Form { get; set; }

    public string SuccessMessage { get; set; }

    public void OnGet()
    {
        ViewData["Title"] = "Liên hệ - Cửa hàng Cà phê";
    }

    public IActionResult OnPost()
    {
        if (!ModelState.IsValid)
            return Page();

        // Xử lý form (gửi email, lưu database, v.v.)
        SuccessMessage = $"Cảm ơn {Form.Name}! Chúng tôi đã nhận được tin nhắn của bạn";
        
        // Clear form
        Form = new ContactForm();
        
        return Page();
    }
}

public class ContactForm
{
    public string Name { get; set; }
    public string Email { get; set; }
    public string Phone { get; set; }
    public string Subject { get; set; }
    public string Message { get; set; }
}
```

---

## 🔨 Cách sử dụng

### Sử dụng View Components trong Views thông thường

**Ví dụ: Cập nhật Home/Index.cshtml**

```html
@{
    ViewData["Title"] = "Trang chủ";
    Layout = null;
}

<!DOCTYPE html>
<html>
<head>
    <title>Cửa hàng Cà phê</title>
</head>
<body>
    <!-- Sử dụng Navbar Component -->
    @await Component.InvokeAsync("Navbar")

    <!-- Nội dung chính -->
    <main class="container">
        <h1>Chào mừng đến với cửa hàng cà phê</h1>
        <!-- ... nội dung khác ... -->
    </main>

    <!-- Sử dụng Best Selling Products Component -->
    @await Component.InvokeAsync("BestSellingProducts")

    <!-- Footer -->
    <footer>© 2024 Cửa hàng Cà phê</footer>
</body>
</html>
```

### Sử dụng View Components trong Layouts

**Ví dụ: Cập nhật _AdminLayout.cshtml**

```html
@{
    Layout = null;
}

<!DOCTYPE html>
<html>
<head>
    <title>Admin Panel</title>
</head>
<body>
    <div class="container-fluid">
        <!-- Navbar Component -->
        @await Component.InvokeAsync("Navbar")

        <div class="row">
            <aside class="col-md-3">
                <!-- Sidebar -->
            </aside>
            <main class="col-md-9">
                @RenderBody()
            </main>
        </div>
    </div>
</body>
</html>
```

### Truy cập Razor Pages

```
- About Page: /Pages/About hoặc /about
- Contact Page: /Pages/Contact hoặc /contact
```

---

## 💡 Ví dụ thực tế

### Ví dụ 1: Thống nhất Status Badge trong Admin Order views

**Trước:**
```html
<!-- Order/Index.cshtml -->
@foreach (var order in Model)
{
    <tr>
        <td>@order.Id</td>
        <td>@order.CustomerName</td>
        <td>
            @switch(order.Status)
            {
                case OrderStatus.Pending:
                    <span class="badge bg-warning">Chờ xử lý</span>
                    break;
                case OrderStatus.Brewing:
                    <span class="badge bg-info">Đang pha</span>
                    break;
                case OrderStatus.Ready:
                    <span class="badge bg-success">Sẵn sàng</span>
                    break;
                // ...
            }
        </td>
    </tr>
}
```

**Sau:**
```html
<!-- Order/Index.cshtml -->
@foreach (var order in Model)
{
    <tr>
        <td>@order.Id</td>
        <td>@order.CustomerName</td>
        <td>
            @await Component.InvokeAsync("StatusBadge", new { status = order.Status })
        </td>
    </tr>
}
```

---

### Ví dụ 2: Thêm Menu vào tất cả trang

**Cập nhật Views/_ViewStart.cshtml hoặc tạo _Layout.cshtml:**

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
</head>
<body>
    <!-- Navbar ở tất cả các trang -->
    @await Component.InvokeAsync("Navbar")

    <!-- Nội dung trang -->
    @RenderBody()

    <!-- Footer -->
    <footer>© 2024</footer>
</body>
</html>
```

---

### Ví dụ 3: Tập hợp View Components trong một trang

**Ví dụ: Home/Index.cshtml (Trang chủ đẹp)**

```html
@{
    ViewData["Title"] = "Trang chủ - Cửa hàng Cà phê";
}

<!-- Navbar -->
@await Component.InvokeAsync("Navbar")

<!-- Hero Section -->
<section class="hero">
    <h1>Cà phê tươi mỗi ngày</h1>
</section>

<!-- Best Selling Products -->
@await Component.InvokeAsync("BestSellingProducts")

<!-- More Sections... -->
```

---

## ⭐ Best Practices

### 1. **View Components**

✅ **Nên:**
- Sử dụng View Components cho các UI logic có thể tái sử dụng
- Giữ View Component logic đơn giản
- Sử dụng caching khi cần thiết:

```csharp
public class BestSellingProductsViewComponent : ViewComponent
{
    private readonly IMemoryCache _cache;
    
    public BestSellingProductsViewComponent(IMemoryCache cache)
    {
        _cache = cache;
    }

    public IViewComponentResult Invoke()
    {
        if (!_cache.TryGetValue("best_selling_products", out var products))
        {
            // Lấy từ database
            products = _context.Products.Take(6).ToList();
            
            // Lưu vào cache 1 giờ
            _cache.Set("best_selling_products", products, 
                TimeSpan.FromHours(1));
        }
        
        return View(products);
    }
}
```

❌ **Không nên:**
- Làm quá nhiều logic phức tạp trong View Component
- Quên inject dependencies
- Sử dụng View Components cho những thứ có thể là Partial viewa

### 2. **Razor Pages**

✅ **Nên:**
- Sử dụng Razor Pages cho các trang độc lập đơn giản
- Following RESTful conventions (OnGet, OnPost, OnPut, OnDelete)
- Xử lý validation trên server-side

```csharp
public class ContactModel : PageModel
{
    [BindProperty]
    [Required(ErrorMessage = "Tên không được để trống")]
    public string Name { get; set; }

    public IActionResult OnPost()
    {
        if (!ModelState.IsValid)
            return Page();
        
        // Xử lý dữ liệu
        return RedirectToPage("Thank You");
    }
}
```

❌ **Không nên:**
- Tạo quá nhiều Razor Pages cho những thứ nên là MVC Controllers
- Mix logic quá nhiều vào PageModel

### 3. **Tổ chức code**

📁 **Cấu trúc dự án:**
```
Views/
├── Components/
│   ├── Navbar/
│   │   ├── NavbarViewComponent.cs
│   │   └── Default.cshtml
│   ├── BestSellingProducts/
│   ├── StatusBadge/
│   └── UserAvatar/
└── ... (Controllers views)

Pages/
├── Shared/
│   └── _PagesLayout.cshtml
├── About.cshtml
├── About.cshtml.cs
├── Contact.cshtml
├── Contact.cshtml.cs
└── ... (other pages)
```

---

## 🚀 Tiếp theo (TODO)

1. **Cải tiến View Components:**
   - Thêm caching
   - Support cho pagination
   - API endpoints cho AJAX loading

2. **Mở rộng Razor Pages:**
   - Product Detail page
   - User Profile page
   - Terms & Conditions page

3. **Tích hợp email:**
   - Gửi email từ Contact form
   - Email notification cho orders

4. **Testing:**
   - Unit tests cho ViewComponents
   - Integration tests cho Razor Pages

---

## 📞 Hỗ trợ

Nếu có vấn đề:
1. Kiểm tra xem `AddRazorPages()` đã được thêm vào Program.cs
2. Kiểm tra xem `MapRazorPages()` đã được thêm vào routing
3. Xem xét đặt tên file và đường dẫn có đúng không
4. Kiểm tra DbContext có được injected chính xác vào View Components

---

**Happy Coding! ☕**
