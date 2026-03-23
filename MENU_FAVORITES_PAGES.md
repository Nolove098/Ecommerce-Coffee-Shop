# 🍽️ New Pages - Menu & Favorites

## 📄 Pages Tạo Mới

### 1. **Menu Page** - Thực Đơn
**Vị trí:** `Pages/Menu.cshtml` + `Pages/Menu.cshtml.cs`
**URLs:**
- `/Pages/Menu`
- `/menu` (short URL)
- Hash style: `/#menu` (SPA style, redirect đến `/Pages/Menu`)

**Tính năng:**
- ✅ Hiển thị toàn bộ sản phẩm từ database
- ✅ Grid layout responsive (1 cột mobile, 2 cột tablet, 3 cột desktop)
- ✅ Hiển thị hình ảnh sản phẩm
- ✅ Giá tiền định dạng Việt Nam (VNĐ)
- ✅ Trạng thái hàng (Hết hàng / Sắp hết / Còn N sản phẩm)
- ✅ Nút "Thêm vào giỏ" (tự động disable nếu hết hàng)
- ✅ Quick links đến trang khác

**Cấu trúc Code:**
```csharp
public class MenuModel : PageModel
{
    private readonly ApplicationDbContext _context;

    public List<SaleStore.Models.Product>? Products { get; set; }

    public MenuModel(ApplicationDbContext context)
    {
        _context = context;
    }

    public void OnGet()
    {
        ViewData["Title"] = "Thực Đơn - Cửa Hàng Cà Phê";
        Products = _context.Products.OrderBy(p => p.Name).ToList();
    }
}
```

---

### 2. **Favorites Page** - Yêu Thích
**Vị trí:** `Pages/Favorites.cshtml` + `Pages/Favorites.cshtml.cs`
**URLs:**
- `/Pages/Favorites`
- `/favorites` (short URL)
- Hash style: `/#favorites` (redirect đến `/Pages/Favorites`)

**Tính năng (Placeholder):**
- ✅ Empty state - Thông báo chưa có sản phẩm yêu thích
- ✅ 3 lợi ích của danh sách yêu thích
- ✅ Quick links đến Menu & Home
- ✅ CTA button để khám phá sản phẩm

**Ghi chú:** Page này cần tích hợp với system lưu favorites của user (localStorage hoặc database). Hiện tại là placeholder.

---

## 🔗 Cập Nhật Navbar

**File:** `Views/Components/Navbar/Default.cshtml`

**Menu Items Mới:**
```html
<a href="/" class="text-gray-700 hover:text-[#6F4E37]">Trang chủ</a>
<a href="/Pages/Menu" class="text-gray-700 hover:text-[#6F4E37]">☕ Thực đơn</a>
<a href="/Pages/About" class="text-gray-700 hover:text-[#6F4E37]">Về chúng tôi</a>
<a href="/Pages/Favorites" class="text-gray-700 hover:text-[#6F4E37]">❤️ Yêu thích</a>
<a href="/Pages/Contact" class="text-gray-700 hover:text-[#6F4E37]">Liên hệ</a>
```

---

## 🧭 Navigation Routing

### Direct URLs:
- **Menu:** `http://localhost:5005/Pages/Menu`
- **About:** `http://localhost:5005/Pages/About`
- **Favorites:** `http://localhost:5005/Pages/Favorites`
- **Contact:** `http://localhost:5005/Pages/Contact`

### Short URLs (ASP.NET Core Razor Pages default):
- **Menu:** `http://localhost:5005/menu`
- **About:** `http://localhost:5005/about`
- **Favorites:** `http://localhost:5005/favorites`
- **Contact:** `http://localhost:5005/contact`

### Hash-style URLs (SPA style - requires client-side routing):
- `http://localhost:5005/#menu` → Redirect to `/menu`
- `http://localhost:5005/#about` → Redirect to `/about`
- `http://localhost:5005/#favorites` → Redirect to `/favorites`
- `http://localhost:5005/#contact` → Redirect to `/contact`

---

## ⚙️ Setup Hash-based Routing (Optional)

Nếu bạn muốn sử dụng hash URLs như `/#menu`, thêm script này vào layout:

```html
<!-- In _PagesLayout.cshtml or master layout -->
<script>
    // Listen for hash changes
    window.addEventListener('hashchange', function() {
        const hash = window.location.hash.substring(1);
        
        // Map hash to URL
        const routes = {
            'menu': '/Pages/Menu',
            'about': '/Pages/About',
            'favorites': '/Pages/Favorites',
            'contact': '/Pages/Contact',
            'home': '/'
        };
        
        if (routes[hash]) {
            window.location.href = routes[hash];
        }
    });
    
    // Check hash on page load
    if (window.location.hash) {
        window.dispatchEvent(new HashChangeEvent('hashchange'));
    }
</script>
```

---

## 🎨 Design Details

### Colors Used:
- Primary: `#D4A574` (Tan/Gold)
- Dark: `#2C1810` (Dark Brown)
- Light: `#FAF8F5` (Cream)
- Secondary: `#6F4E37` (Medium Brown)

### Responsive Breakpoints:
- **Mobile:** 1 column grid
- **Tablet (md):** 2 columns
- **Desktop (lg):** 3 columns

---

## 📋 Checklist

- [x] Create Menu.cshtml page
- [x] Create Favorites.cshtml page
- [x] Update Navbar with new links
- [x] Add Database query in Menu PageModel
- [x] Add responsive grid layout
- [x] Add stock status badges
- [x] Add product images & pricing
- [x] Add quick links
- [ ] Implement add to cart functionality
- [ ] Implement favorites/wishlist storage
- [ ] Add notification system for price drops

---

## 🚀 Next Steps (TODO)

### 1. Implement Favorites System
```csharp
// Add to User model or create separate Wishlist table
public class Wishlist
{
    public int Id { get; set; }
    public string UserId { get; set; }
    public int ProductId { get; set; }
    public DateTime AddedAt { get; set; }
    
    public AppUser User { get; set; }
    public Product Product { get; set; }
}
```

### 2. Add localStorage for Guest Users
```javascript
// Save favorites to browser storage
function addToFavorites(productId) {
    let favorites = JSON.parse(localStorage.getItem('favorites')) || [];
    if (!favorites.includes(productId)) {
        favorites.push(productId);
        localStorage.setItem('favorites', JSON.stringify(favorites));
    }
}
```

### 3. Implement Add to Cart
```csharp
[HttpPost]
public IActionResult AddToCart(int productId, int quantity)
{
    // Get product from database
    // Add to session cart
    // Return result
}
```

---

## 🧪 Testing URLs

**Try these URLs in browser:**
1. http://localhost:5005/Pages/Menu
2. http://localhost:5005/menu
3. http://localhost:5005/Pages/Favorites
4. http://localhost:5005/favorites
5. Click navbar menu items

---

**Hoàn thành! Các trang già đã được tạo với navbar mới. ☕**
