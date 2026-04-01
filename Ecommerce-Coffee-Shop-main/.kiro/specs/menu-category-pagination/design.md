# Design Document: Menu Category Pagination

## Overview

This design implements category-based filtering and pagination for the Menu page in the SaleStore coffee shop application. The solution extends the existing Razor Page architecture to support efficient product browsing through a sidebar navigation and paginated product display.

### Key Design Goals

- Maintain consistency with existing Bootstrap 5.3 + custom CSS design system
- Implement server-side pagination for optimal database performance
- Provide responsive mobile experience with collapsible sidebar
- Preserve all existing functionality (cart integration, product details, etc.)
- Use query string parameters for shareable, bookmarkable URLs

### Technology Stack

- **Backend**: ASP.NET Core 6.0 Razor Pages with Entity Framework Core
- **Frontend**: Bootstrap 5.3 with custom CSS (no Tailwind despite requirements mentioning it)
- **Database**: SQL Server via Entity Framework Core
- **State Management**: Query string parameters (category, page)

## Architecture

### Component Structure

```
Pages/Menu.cshtml (View)
├── Hero Banner (existing)
├── Category Sidebar (new)
│   ├── "Tất cả" option
│   ├── Category list with counts
│   └── Mobile toggle button
├── Product Grid (modified)
│   ├── Product count display
│   ├── Product cards (existing)
│   └── Empty state message
└── Pagination Controls (new)
    ├── Previous button
    ├── Page indicator
    └── Next button

Pages/Menu.cshtml.cs (PageModel)
├── Query Parameters (category, page)
├── Database Query Logic
│   ├── Category filtering
│   ├── Pagination (Skip/Take)
│   └── Category statistics
├── View Properties
│   ├── Products (List<Product>)
│   ├── Categories (List<CategoryInfo>)
│   ├── CurrentPage, TotalPages
│   └── SelectedCategory
└── Input Validation
```

### Data Flow

1. **Request**: User clicks category or pagination control
2. **URL Update**: Browser navigates to `/Menu?category=X&page=Y`
3. **PageModel Binding**: Query parameters bind to properties
4. **Validation**: Invalid parameters are corrected (redirect if needed)
5. **Database Query**: EF Core executes filtered + paginated query
6. **View Rendering**: Razor template renders products and controls
7. **Response**: HTML sent to browser with updated state

### Database Query Strategy

The design uses efficient EF Core queries to minimize database load:

```csharp
// Efficient pagination with filtering
var query = _context.Products
    .Where(p => p.IsActive)
    .Where(p => category == null || p.Category == category);

var totalCount = await query.CountAsync();
var products = await query
    .OrderBy(p => p.Name)
    .Skip((page - 1) * pageSize)
    .Take(pageSize)
    .ToListAsync();
```

This approach:
- Filters inactive products first
- Applies category filter if specified
- Counts total for pagination calculation
- Uses Skip/Take for efficient pagination (SQL OFFSET/FETCH)
- Orders consistently for predictable results

## Components and Interfaces

### 1. MenuModel (PageModel)

**Location**: `Pages/Menu.cshtml.cs`

**Properties**:

```csharp
public class MenuModel : PageModel
{
    // Dependencies
    private readonly ApplicationDbContext _context;
    private const int PageSize = 12;

    // Query Parameters
    [BindProperty(SupportsGet = true)]
    public string? Category { get; set; }

    [BindProperty(SupportsGet = true)]
    public int Page { get; set; } = 1;

    // View Data
    public List<Product> Products { get; set; } = new();
    public List<CategoryInfo> Categories { get; set; } = new();
    public int CurrentPage { get; set; }
    public int TotalPages { get; set; }
    public int TotalProducts { get; set; }
    public string? SelectedCategory { get; set; }
}
```

**Methods**:

- `OnGetAsync()`: Main handler for GET requests
  - Validates and normalizes query parameters
  - Fetches category statistics
  - Executes filtered and paginated product query
  - Calculates pagination metadata
  - Returns Page() or RedirectToPage() for invalid params

**Validation Logic**:

```csharp
// Normalize page to valid range
if (Page < 1)
{
    return RedirectToPage("/Menu", new { category = Category, page = 1 });
}

if (Page > TotalPages && TotalPages > 0)
{
    return RedirectToPage("/Menu", new { category = Category, page = TotalPages });
}
```

### 2. CategoryInfo (Helper Class)

**Purpose**: Transfer category data with product counts to view

```csharp
public class CategoryInfo
{
    public string Name { get; set; } = null!;
    public int ProductCount { get; set; }
}
```

**Usage**: Populated via LINQ group query:

```csharp
Categories = await _context.Products
    .Where(p => p.IsActive)
    .GroupBy(p => p.Category)
    .Select(g => new CategoryInfo 
    { 
        Name = g.Key, 
        ProductCount = g.Count() 
    })
    .OrderBy(c => c.Name)
    .ToListAsync();
```

### 3. Category Sidebar Component

**Location**: `Pages/Menu.cshtml` (inline)

**Structure**:

```html
<aside class="category-sidebar">
  <h3>Danh mục</h3>
  <ul>
    <li class="@(Model.SelectedCategory == null ? "active" : "")">
      <a href="/Menu">Tất cả (@Model.TotalProducts)</a>
    </li>
    @foreach(var cat in Model.Categories)
    {
      <li class="@(Model.SelectedCategory == cat.Name ? "active" : "")">
        <a href="/Menu?category=@cat.Name">
          @cat.Name (@cat.ProductCount)
        </a>
      </li>
    }
  </ul>
</aside>
```

**Styling**: Bootstrap classes with custom CSS for:
- Fixed width on desktop (250px)
- Sticky positioning
- Active state highlighting
- Mobile collapse/expand behavior

### 4. Pagination Controls Component

**Location**: `Pages/Menu.cshtml` (inline)

**Structure**:

```html
@if(Model.TotalPages > 1)
{
  <nav class="pagination-controls">
    <a href="/Menu?category=@Model.Category&page=@(Model.CurrentPage-1)"
       class="btn btn-outline-primary @(Model.CurrentPage == 1 ? "disabled" : "")">
      Trước
    </a>
    <span>Trang @Model.CurrentPage / @Model.TotalPages</span>
    <a href="/Menu?category=@Model.Category&page=@(Model.CurrentPage+1)"
       class="btn btn-outline-primary @(Model.CurrentPage == Model.TotalPages ? "disabled" : "")">
      Sau
    </a>
  </nav>
}
```

**Features**:
- Conditional rendering (only show if multiple pages)
- Disabled state for boundary pages
- Preserves category filter in URLs
- Centered layout with flexbox

### 5. Mobile Sidebar Toggle

**Location**: `Pages/Menu.cshtml` (JavaScript)

**Implementation**:

```javascript
// Mobile sidebar toggle
const sidebarToggle = document.getElementById('sidebarToggle');
const sidebar = document.querySelector('.category-sidebar');

sidebarToggle?.addEventListener('click', () => {
  sidebar.classList.toggle('show');
});

// Auto-close on category selection (mobile)
document.querySelectorAll('.category-sidebar a').forEach(link => {
  link.addEventListener('click', () => {
    if (window.innerWidth < 768) {
      sidebar.classList.remove('show');
    }
  });
});
```

**CSS**:

```css
@media (max-width: 767px) {
  .category-sidebar {
    position: fixed;
    left: -100%;
    transition: left 0.3s;
    z-index: 1040;
  }
  .category-sidebar.show {
    left: 0;
  }
}
```

## Data Models

### Product Model (Existing)

**Location**: `Models/Product.cs`

```csharp
public class Product
{
    public long Id { get; set; }
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
    public decimal Price { get; set; }
    public string Category { get; set; } = null!;  // Used for filtering
    public string? ImageUrl { get; set; }
    public int Stock { get; set; }
    public bool IsActive { get; set; }  // Used to filter out inactive products
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
```

**Notes**:
- Category property already exists (no migration needed)
- IsActive flag used to exclude deleted/inactive products
- No foreign key relationship (categories are simple strings)

### CategoryInfo Model (New)

**Location**: `Models/ViewModels/CategoryInfo.cs` (to be created)

```csharp
namespace SaleStore.Models.ViewModels
{
    public class CategoryInfo
    {
        public string Name { get; set; } = null!;
        public int ProductCount { get; set; }
    }
}
```

**Purpose**: View model for transferring category statistics from PageModel to View

## Correctness Properties

*A property is a characteristic or behavior that should hold true across all valid executions of a system—essentially, a formal statement about what the system should do. Properties serve as the bridge between human-readable specifications and machine-verifiable correctness guarantees.*


### Property 1: Sidebar displays all and only active product categories with accurate counts

*For any* set of products in the database, the sidebar should display exactly the unique categories from active products (IsActive=true), each with a count matching the number of active products in that category, and should not display categories with zero active products.

**Validates: Requirements 1.2, 1.4, 1.6**

### Property 2: Category filtering displays only matching products

*For any* selected category, all displayed products should have that exact category value, and all products should have IsActive=true.

**Validates: Requirements 2.1, 2.4, 2.5**

### Property 3: URL state synchronization

*For any* combination of category and page number, the URL query string should contain both parameters accurately, and loading a page with those parameters should restore that exact filter and pagination state.

**Validates: Requirements 2.3, 2.4, 3.6, 4.5**

### Property 4: Page size limit enforcement

*For any* page with sufficient products available, the number of displayed products should not exceed Page_Size (12), and the last page should display only the remaining products without empty placeholders.

**Validates: Requirements 3.1, 8.4**

### Property 5: Pagination controls visibility

*For any* filter state, pagination controls should be visible if and only if the total filtered product count exceeds Page_Size.

**Validates: Requirements 3.2**

### Property 6: Pagination button boundary states

*For any* page number, the Previous button should be disabled when on page 1, and the Next button should be disabled when on the last page.

**Validates: Requirements 3.4, 3.5**

### Property 7: Category change resets pagination

*For any* category change, the page number should reset to 1, preventing users from being on an invalid page number for the new category's product count.

**Validates: Requirements 4.4**

### Property 8: Active category highlighting

*For any* selected category, the corresponding sidebar item should have an "active" CSS class or styling marker, and no other category should have this marker.

**Validates: Requirements 1.5**

### Property 9: Product display completeness

*For any* displayed product, the rendered HTML should contain all required fields (image or placeholder, name, description, price, stock status, add-to-cart button, detail page link).

**Validates: Requirements 7.1, 7.2, 7.5**

### Property 10: Pagination metadata accuracy

*For any* page state, the displayed pagination text should show the correct current page and total page count in the format "Trang X / Y", and the product count display should match the actual number of filtered products.

**Validates: Requirements 8.1, 8.2**

## Error Handling

### Invalid Query Parameters

**Page Number Validation**:
- **page < 1**: Redirect to page 1 with same category filter
- **page > totalPages**: Redirect to last valid page with same category filter
- **page not an integer**: Default to page 1 (ASP.NET Core model binding handles this)

**Category Validation**:
- **category doesn't exist**: Treat as "All" filter (show all products)
- **category is null/empty**: Treat as "All" filter

**Implementation**:

```csharp
public async Task<IActionResult> OnGetAsync()
{
    // Validate page parameter
    if (Page < 1)
    {
        return RedirectToPage("/Menu", new { category = Category, page = 1 });
    }

    // Get filtered products
    var query = _context.Products.Where(p => p.IsActive);
    
    if (!string.IsNullOrEmpty(Category))
    {
        query = query.Where(p => p.Category == Category);
    }

    TotalProducts = await query.CountAsync();
    TotalPages = (int)Math.Ceiling(TotalProducts / (double)PageSize);

    // Validate page doesn't exceed total
    if (Page > TotalPages && TotalPages > 0)
    {
        return RedirectToPage("/Menu", new { category = Category, page = TotalPages });
    }

    // Continue with query...
}
```

### Database Errors

**Connection Failures**:
- Display user-friendly error message: "Không thể tải sản phẩm. Vui lòng thử lại."
- Log error details for debugging
- Return empty product list to prevent page crash

**Query Timeouts**:
- Implement reasonable timeout (30 seconds)
- Consider adding database indexes on Category and IsActive columns
- Log slow queries for optimization

### Empty States

**No Products Found**:
- Display message: "Không tìm thấy sản phẩm"
- Show suggestion to try different category
- Hide pagination controls

**No Categories Available**:
- Hide sidebar (or show message if no products exist at all)
- Display all products in grid

### Concurrent Data Changes

**Products Added/Removed During Browsing**:
- Accept eventual consistency (user may see slightly stale data)
- Page refresh will show updated data
- No special handling needed (stateless design)

**Category Changes**:
- If a product's category changes while user is viewing it, next navigation will reflect new state
- No locking or transaction isolation needed

## Testing Strategy

### Dual Testing Approach

This feature requires both unit testing and property-based testing for comprehensive coverage:

- **Unit tests**: Verify specific examples, edge cases, and error conditions
- **Property tests**: Verify universal properties across all inputs
- Together: Comprehensive coverage (unit tests catch concrete bugs, property tests verify general correctness)

### Unit Testing

**Framework**: xUnit with ASP.NET Core testing utilities

**Test Categories**:

1. **PageModel Logic Tests**:
   - Test OnGetAsync with valid category and page parameters
   - Test redirect behavior for invalid page numbers
   - Test category filtering with known product sets
   - Test pagination calculation with various product counts
   - Test empty state handling

2. **Edge Case Tests**:
   - Page number = 0 (should redirect to 1)
   - Page number > total pages (should redirect to last page)
   - Negative page numbers (should redirect to 1)
   - Non-existent category (should show all products)
   - Empty category string (should show all products)
   - Exactly Page_Size products (should show 1 page, no pagination)
   - Page_Size + 1 products (should show 2 pages)

3. **Integration Tests**:
   - Test with in-memory database
   - Verify EF Core query generation
   - Test category statistics calculation
   - Test product filtering and pagination together

**Example Unit Test**:

```csharp
[Fact]
public async Task OnGetAsync_WithInvalidPageNumber_RedirectsToPageOne()
{
    // Arrange
    var options = new DbContextOptionsBuilder<ApplicationDbContext>()
        .UseInMemoryDatabase(databaseName: "TestDb")
        .Options;
    
    using var context = new ApplicationDbContext(options);
    var pageModel = new MenuModel(context) { Page = -1 };

    // Act
    var result = await pageModel.OnGetAsync();

    // Assert
    var redirectResult = Assert.IsType<RedirectToPageResult>(result);
    Assert.Equal("/Menu", redirectResult.PageName);
    Assert.Equal(1, redirectResult.RouteValues["page"]);
}
```

### Property-Based Testing

**Framework**: FsCheck (F# property testing library with C# support)

**Configuration**:
- Minimum 100 iterations per property test
- Each test references its design document property
- Tag format: **Feature: menu-category-pagination, Property {number}: {property_text}**

**Property Test Examples**:

1. **Property 1: Sidebar Category Accuracy**

```csharp
// Feature: menu-category-pagination, Property 1: Sidebar displays all and only active product categories with accurate counts
[Property(MaxTest = 100)]
public Property SidebarDisplaysActiveCategories()
{
    return Prop.ForAll(
        GenerateProductList(),
        async products =>
        {
            // Arrange
            var context = CreateContextWithProducts(products);
            var pageModel = new MenuModel(context);

            // Act
            await pageModel.OnGetAsync();

            // Assert
            var activeProducts = products.Where(p => p.IsActive).ToList();
            var expectedCategories = activeProducts
                .GroupBy(p => p.Category)
                .Select(g => new { Category = g.Key, Count = g.Count() })
                .OrderBy(x => x.Category)
                .ToList();

            Assert.Equal(expectedCategories.Count, pageModel.Categories.Count);
            
            foreach (var expected in expectedCategories)
            {
                var actual = pageModel.Categories.FirstOrDefault(c => c.Name == expected.Category);
                Assert.NotNull(actual);
                Assert.Equal(expected.Count, actual.ProductCount);
            }
        });
}
```

2. **Property 2: Category Filtering**

```csharp
// Feature: menu-category-pagination, Property 2: Category filtering displays only matching products
[Property(MaxTest = 100)]
public Property CategoryFilteringShowsOnlyMatchingProducts()
{
    return Prop.ForAll(
        GenerateProductList(),
        Gen.Elements(new[] { "Cà phê", "Trà", "Bánh", "Khác" }),
        async (products, selectedCategory) =>
        {
            // Arrange
            var context = CreateContextWithProducts(products);
            var pageModel = new MenuModel(context) { Category = selectedCategory };

            // Act
            await pageModel.OnGetAsync();

            // Assert
            Assert.All(pageModel.Products, p =>
            {
                Assert.Equal(selectedCategory, p.Category);
                Assert.True(p.IsActive);
            });
        });
}
```

3. **Property 4: Page Size Limit**

```csharp
// Feature: menu-category-pagination, Property 4: Page size limit enforcement
[Property(MaxTest = 100)]
public Property PageSizeLimitEnforced()
{
    return Prop.ForAll(
        GenerateProductList(),
        Gen.Choose(1, 10),
        async (products, pageNumber) =>
        {
            // Arrange
            var context = CreateContextWithProducts(products);
            var pageModel = new MenuModel(context) { Page = pageNumber };

            // Act
            await pageModel.OnGetAsync();

            // Assert
            var activeCount = products.Count(p => p.IsActive);
            var expectedOnPage = Math.Min(12, Math.Max(0, activeCount - (pageNumber - 1) * 12));
            
            Assert.True(pageModel.Products.Count <= 12);
            if (pageNumber <= pageModel.TotalPages)
            {
                Assert.Equal(expectedOnPage, pageModel.Products.Count);
            }
        });
}
```

4. **Property 3: URL State Synchronization**

```csharp
// Feature: menu-category-pagination, Property 3: URL state synchronization
[Property(MaxTest = 100)]
public Property UrlStatePreservesFilterAndPage()
{
    return Prop.ForAll(
        Gen.Elements(new[] { "Cà phê", "Trà", "Bánh", null }),
        Gen.Choose(1, 5),
        async (category, page) =>
        {
            // Arrange
            var context = CreateContextWithProducts(GenerateSampleProducts());
            var pageModel = new MenuModel(context) 
            { 
                Category = category, 
                Page = page 
            };

            // Act
            await pageModel.OnGetAsync();

            // Assert
            Assert.Equal(category, pageModel.SelectedCategory);
            Assert.Equal(page, pageModel.CurrentPage);
        });
}
```

**Generator Functions**:

```csharp
private static Arbitrary<List<Product>> GenerateProductList()
{
    return Arb.From(
        from count in Gen.Choose(0, 50)
        from products in Gen.ListOf(count, GenerateProduct())
        select products.ToList()
    );
}

private static Gen<Product> GenerateProduct()
{
    return from id in Gen.Choose(1, 1000)
           from name in Gen.Elements(new[] { "Cà phê đen", "Cà phê sữa", "Trà xanh", "Bánh mì" })
           from category in Gen.Elements(new[] { "Cà phê", "Trà", "Bánh", "Khác" })
           from price in Gen.Choose(10000, 100000)
           from isActive in Arb.Generate<bool>()
           select new Product
           {
               Id = id,
               Name = name,
               Category = category,
               Price = price,
               IsActive = isActive,
               Stock = 10,
               CreatedAt = DateTime.Now,
               UpdatedAt = DateTime.Now
           };
}
```

### End-to-End Testing

**Framework**: Playwright (already in use per project structure)

**Test Scenarios**:
- Navigate through categories and verify products update
- Click pagination controls and verify page changes
- Test mobile sidebar toggle functionality
- Verify responsive layout at different breakpoints
- Test add-to-cart functionality from paginated pages
- Verify URL updates when navigating

**Example E2E Test**:

```javascript
// tests/menu-pagination.spec.js
test('category filtering updates products', async ({ page }) => {
  await page.goto('/Menu');
  
  // Click a category
  await page.click('text=Cà phê');
  
  // Verify URL updated
  await expect(page).toHaveURL(/category=Cà phê/);
  
  // Verify all products have correct category
  const categoryBadges = await page.locator('.badge').allTextContents();
  categoryBadges.forEach(badge => {
    expect(badge).toBe('Cà phê');
  });
});

test('pagination controls work correctly', async ({ page }) => {
  await page.goto('/Menu');
  
  // Click next page
  await page.click('text=Sau');
  
  // Verify URL updated
  await expect(page).toHaveURL(/page=2/);
  
  // Verify page indicator updated
  await expect(page.locator('text=Trang 2')).toBeVisible();
});
```

### Performance Testing

**Metrics to Monitor**:
- Database query execution time (should be < 100ms)
- Page load time (should be < 2 seconds)
- Time to First Contentful Paint (should be < 1 second)

**Load Testing**:
- Test with 1000+ products in database
- Verify pagination performance doesn't degrade
- Monitor database query plans

**Optimization Strategies**:
- Add database indexes on `Category` and `IsActive` columns
- Consider caching category statistics (if categories are stable)
- Use `AsNoTracking()` for read-only queries

## Implementation Notes

### Database Indexes

For optimal performance, add these indexes to the Products table:

```sql
CREATE INDEX IX_Products_Category ON Products(Category) WHERE IsActive = 1;
CREATE INDEX IX_Products_IsActive_Category ON Products(IsActive, Category);
```

### CSS Customization

The design uses Bootstrap 5.3 as the base framework. Custom CSS additions needed:

```css
/* Category Sidebar */
.category-sidebar {
  width: 250px;
  position: sticky;
  top: 88px; /* Below fixed navbar */
  max-height: calc(100vh - 104px);
  overflow-y: auto;
}

.category-sidebar ul {
  list-style: none;
  padding: 0;
}

.category-sidebar li {
  margin-bottom: 0.5rem;
}

.category-sidebar a {
  display: block;
  padding: 0.75rem 1rem;
  border-radius: 0.5rem;
  text-decoration: none;
  color: #6c757d;
  transition: all 0.2s;
}

.category-sidebar a:hover {
  background-color: #f8f9fa;
  color: #1a0e08;
}

.category-sidebar li.active a {
  background-color: #1a0e08;
  color: white;
  font-weight: 600;
}

/* Mobile Sidebar */
@media (max-width: 767px) {
  .category-sidebar {
    position: fixed;
    left: -100%;
    top: 72px;
    bottom: 0;
    width: 280px;
    background: white;
    z-index: 1040;
    transition: left 0.3s ease;
    box-shadow: 2px 0 8px rgba(0,0,0,0.1);
    padding: 1rem;
  }
  
  .category-sidebar.show {
    left: 0;
  }
  
  .sidebar-overlay {
    position: fixed;
    top: 0;
    left: 0;
    right: 0;
    bottom: 0;
    background: rgba(0,0,0,0.5);
    z-index: 1039;
    display: none;
  }
  
  .sidebar-overlay.show {
    display: block;
  }
}

/* Pagination Controls */
.pagination-controls {
  display: flex;
  align-items: center;
  justify-content: center;
  gap: 1rem;
  margin-top: 2rem;
  padding: 1rem;
}

.pagination-controls .btn.disabled {
  pointer-events: none;
  opacity: 0.5;
}
```

### Accessibility Considerations

- Use semantic HTML (`<nav>`, `<aside>`, `<main>`)
- Add ARIA labels to pagination controls
- Ensure keyboard navigation works for sidebar and pagination
- Maintain focus management when navigating pages
- Use sufficient color contrast for active states

### Future Enhancements

Potential improvements for future iterations:

1. **Advanced Pagination**: Add page number buttons (1, 2, 3...) instead of just Previous/Next
2. **URL-friendly Categories**: Use slugs instead of Vietnamese text in URLs
3. **Sorting Options**: Allow sorting by price, name, popularity
4. **Filter Persistence**: Remember user's last selected category in localStorage
5. **Infinite Scroll**: Alternative to pagination for mobile users
6. **Category Icons**: Add visual icons for each category
7. **Product Count Animation**: Animate count changes when filtering
8. **Search Integration**: Add search bar that works with category filtering

