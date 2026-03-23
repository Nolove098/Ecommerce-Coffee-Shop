# 🔗 Hash-Based Navigation Guide

Nếu bạn muốn URLs dạng `http://localhost:5005/#menu` thay vì `http://localhost:5005/Pages/Menu`, hãy follow hướng dẫn này.

## 🎯 2 Cách Implement

### **Cách 1: JavaScript Redirect (Client-side)**

Thêm script này vào `Pages/Shared/_PagesLayout.cshtml`:

```html
<!DOCTYPE html>
<html>
<head>
    <!-- ... existing head content ... -->
</head>
<body>
    @await Component.InvokeAsync("Navbar")
    
    @RenderBody()
    
    <!-- Footer & scripts -->
    
    <script>
        // Hash-based routing
        const routes = {
            'menu': '/Pages/Menu',
            'about': '/Pages/About',
            'favorites': '/Pages/Favorites',
            'contact': '/Pages/Contact'
        };

        // Listen for hash changes
        window.addEventListener('hashchange', function() {
            const hash = window.location.hash.substring(1);
            if (routes[hash]) {
                window.location.href = routes[hash];
            }
        });

        // Check hash on page load
        window.addEventListener('load', function() {
            const hash = window.location.hash.substring(1);
            if (hash && routes[hash]) {
                window.location.href = routes[hash];
            }
        });
    </script>
    
    @await RenderSectionAsync("Scripts", required: false)
</body>
</html>
```

**Kết quả:**
- `http://localhost:5005/#menu` → Redirect to `/Pages/Menu`
- `http://localhost:5005/#about` → Redirect to `/Pages/About`
- `http://localhost:5005/#favorites` → Redirect to `/Pages/Favorites`
- `http://localhost:5005/#contact` → Redirect to `/Pages/Contact`

---

### **Cách 2: Create Hash-friendly Home Page**

Nếu bạn muốn **Home page hiển thị tất cả sections** (Thực đơn, Giới thiệu, Yêu thích, Liên hệ) trong **một page duy nhất** với hash navigation:

#### File: `Views/Home/Index.cshtml`

```html
@{
    ViewData["Title"] = "Trang Chủ - Cửa Hàng Cà Phê";
    Layout = null;
}

<!DOCTYPE html>
<html lang="vi">
<head>
    <meta charset="UTF-8">
    <meta name="viewport" content="width=device-width, initial-scale=1.0">
    <title>@ViewData["Title"]</title>
    <script src="https://cdn.tailwindcss.com"></script>
</head>
<body class="bg-[#FAF8F5]">
    <!-- Navbar -->
    @await Component.InvokeAsync("Navbar")

    <!-- Home Section -->
    <section id="home" class="py-20 px-4 bg-gradient-to-r from-[#6F4E37] to-[#2C1810] text-white">
        <div class="max-w-7xl mx-auto text-center">
            <h1 class="text-5xl font-bold mb-4">☕ Cà Phê Tươi Mỗi Ngày</h1>
            <p class="text-xl text-gray-300 mb-8">Hương vị cao cấp, chất lượng đảm bảo</p>
            <a href="#menu" class="inline-block px-8 py-3 bg-[#D4A574] hover:bg-[#C89968] rounded-lg font-semibold transition">
                Xem Thực Đơn
            </a>
        </div>
    </section>

    <!-- Menu Section -->
    <section id="menu">
        @await Component.InvokeAsync("BestSellingProducts")
    </section>

    <!-- About Section -->
    <section id="about" class="py-16 bg-white">
        <div class="max-w-4xl mx-auto px-4">
            <h2 class="text-3xl font-bold text-[#2C1810] mb-6">Về Chúng Tôi</h2>
            <p class="text-gray-700 mb-4">
                Cửa hàng Cà phê của chúng tôi được thành lập vào năm 2020 với sứ mệnh mang những 
                tách cà phê chất lượng cao nhất đến tay khách hàng.
            </p>
            <p class="text-gray-700">
                Bắt đầu từ một quán nhỏ, ngày nay chúng tôi phục vụ hàng ngàn khách hàng hài lòng.
            </p>
            <a href="/Pages/About" class="inline-block mt-4 px-6 py-2 bg-[#D4A574] text-white rounded-lg hover:bg-[#C89968] transition">
                Tìm Hiểu Thêm
            </a>
        </div>
    </section>

    <!-- Favorites Section -->
    <section id="favorites" class="py-16 bg-[#FAF8F5]">
        <div class="max-w-4xl mx-auto px-4 text-center">
            <h2 class="text-3xl font-bold text-[#2C1810] mb-6">❤️ Sản Phẩm Yêu Thích</h2>
            <p class="text-gray-700 mb-8">
                Lưu những sản phẩm yêu thích để mua lại nhanh hơn
            </p>
            <a href="/Pages/Favorites" class="inline-block px-6 py-2 bg-[#D4A574] text-white rounded-lg hover:bg-[#C89968] transition">
                Xem Yêu Thích
            </a>
        </div>
    </section>

    <!-- Contact Section -->
    <section id="contact" class="py-16 bg-white">
        <div class="max-w-4xl mx-auto px-4 text-center">
            <h2 class="text-3xl font-bold text-[#2C1810] mb-6">Liên Hệ Với Chúng Tôi</h2>
            <p class="text-gray-700 mb-4">
                📍 123 Đường Cà phê, TP.HCM<br>
                📞 (028) 1234 5678<br>
                📧 hello@coffeeshop.vn
            </p>
            <a href="/Pages/Contact" class="inline-block mt-4 px-6 py-2 bg-[#D4A574] text-white rounded-lg hover:bg-[#C89968] transition">
                Gửi Tin Nhắn
            </a>
        </div>
    </section>

    <!-- Footer -->
    <footer class="bg-[#2C1810] text-white py-8">
        <div class="max-w-7xl mx-auto px-4 text-center">
            <p>&copy; 2024 Cửa Hàng Cà Phê. All rights reserved.</p>
        </div>
    </footer>

    <script>
        // Smooth scrolling for anchor links
        document.querySelectorAll('a[href^="#"]').forEach(anchor => {
            anchor.addEventListener('click', function (e) {
                const href = this.getAttribute('href');
                if (href !== '#') {
                    e.preventDefault();
                    const target = document.querySelector(href);
                    if (target) {
                        target.scrollIntoView({ behavior: 'smooth' });
                        // Update URL hash
                        window.history.pushState(null, null, href);
                    }
                }
            });
        });
    </script>
</body>
</html>
```

**URLs sẽ hoạt động như:**
- `http://localhost:5005/#home` → Scroll to home section
- `http://localhost:5005/#menu` → Scroll to menu section
- `http://localhost:5005/#about` → Scroll to about section
- `http://localhost:5005/#favorites` → Scroll to favorites section
- `http://localhost:5005/#contact` → Scroll to contact section

---

## 🔄 Hybrid Approach (Recommended)

Bạn có thể **kết hợp cả hai** - Home page có hash sections + Separate pages:

```html
<!-- Navbar href -->
<a href="/#menu">Thực Đơn</a>        <!-- Scroll on home -->
<a href="/Pages/Menu">Thực Đơn</a>  <!-- Full page elsewhere -->
```

**Lợi ích:**
- ✅ Fast hash navigation on home page
- ✅ Full pages for detailed views
- ✅ Smooth user experience
- ✅ All content accessible via multiple routes

---

## 📊 So Sánh 3 Cách

| Feature | Hash Navigation | Separate Pages | Hybrid |
|---------|---|---|---|
| Fast navigation | ⭐⭐⭐⭐⭐ | ⭐⭐ | ⭐⭐⭐⭐⭐ |
| SEO friendly | ⭐ | ⭐⭐⭐⭐⭐ | ⭐⭐⭐ |
| Bookmarkable | ⭐⭐ | ⭐⭐⭐⭐⭐ | ⭐⭐⭐⭐⭐ |
| Load time | ⭐⭐⭐⭐⭐ | ⭐⭐ | ⭐⭐⭐⭐ |
| Complexity | ⭐ | ⭐⭐⭐ | ⭐⭐⭐ |

---

## ✅ My Recommendation

**Chọn Hybrid Approach:**
1. Navbar links to `/Pages/Menu`, `/Pages/About`, v.v.
2. Each page is a full Razor Page
3. Add hash navigation on Home page sections (optional enhancement)
4. Best of both worlds!

---

**Chúc mừng! Giờ bạn có cách linh hoạt để điều hướng! ☕**
