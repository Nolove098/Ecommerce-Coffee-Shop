 # Implementation Plan: Menu Category Pagination

## Overview

This plan implements category-based filtering and pagination for the Menu page using ASP.NET Core Razor Pages with Entity Framework Core. The implementation adds a sidebar for category navigation, server-side pagination with query string parameters, and responsive mobile support while maintaining all existing functionality.

## Tasks

- [x] 1. Create CategoryInfo view model
  - Create `Models/ViewModels/CategoryInfo.cs` with Name and ProductCount properties
  - _Requirements: 1.2, 1.4_

- [x] 2. Update MenuModel PageModel with filtering and pagination logic
  - [x] 2.1 Add query parameter properties and view data properties
    - Add `[BindProperty(SupportsGet = true)]` for Category and Page parameters
    - Add properties: Products, Categories, CurrentPage, TotalPages, TotalProducts, SelectedCategory
    - Set PageSize constant to 12
    - _Requirements: 2.3, 2.4, 3.1, 3.6_

  - [x] 2.2 Implement category statistics query
    - Query active products grouped by category
    - Project to CategoryInfo list with counts
    - Order by category name
    - _Requirements: 1.2, 1.4, 1.6_

  - [x] 2.3 Implement filtered and paginated product query
    - Filter by IsActive = true
    - Apply category filter if Category parameter is not null/empty
    - Calculate total count and total pages
    - Apply Skip/Take for pagination with OrderBy
    - _Requirements: 2.1, 2.4, 2.5, 3.1, 4.1, 4.2_

  - [x] 2.4 Add query parameter validation and redirect logic
    - Redirect to page 1 if Page < 1
    - Redirect to last page if Page > TotalPages
    - Handle non-existent categories by showing all products
    - _Requirements: 5.1, 5.2, 5.3, 5.4_

  - [ ]* 2.5 Write property test for sidebar category accuracy
    - **Property 1: Sidebar displays all and only active product categories with accurate counts**
    - **Validates: Requirements 1.2, 1.4, 1.6**

  - [ ]* 2.6 Write property test for category filtering
    - **Property 2: Category filtering displays only matching products**
    - **Validates: Requirements 2.1, 2.4, 2.5**

  - [ ]* 2.7 Write property test for URL state synchronization
    - **Property 3: URL state synchronization**
    - **Validates: Requirements 2.3, 2.4, 3.6, 4.5**

  - [ ]* 2.8 Write property test for page size limit enforcement
    - **Property 4: Page size limit enforcement**
    - **Validates: Requirements 3.1, 8.4**

  - [ ]* 2.9 Write unit tests for PageModel logic
    - Test OnGetAsync with valid category and page parameters
    - Test redirect for page < 1
    - Test redirect for page > total pages
    - Test non-existent category shows all products
    - Test pagination calculation with various product counts
    - _Requirements: 5.1, 5.2, 5.3, 5.4_

- [x] 3. Checkpoint - Ensure backend logic tests pass
  - Ensure all tests pass, ask the user if questions arise.

- [x] 4. Update Menu.cshtml view with sidebar and pagination UI
  - [x] 4.1 Add category sidebar component
    - Create aside element with "Tất cả" option linking to /Menu
    - Loop through Model.Categories to display category links with counts
    - Add active class to selected category using Model.SelectedCategory
    - Use Bootstrap classes and prepare for custom CSS
    - _Requirements: 1.1, 1.2, 1.3, 1.4, 1.5_

  - [x] 4.2 Add mobile sidebar toggle button
    - Add button visible only on mobile (< 768px) to toggle sidebar
    - Position button appropriately in layout
    - _Requirements: 6.2_

  - [x] 4.3 Update product grid section
    - Add product count display above grid showing Model.TotalProducts
    - Add empty state message when Model.Products is empty
    - Ensure product cards preserve existing functionality (images, prices, add-to-cart, links)
    - _Requirements: 7.1, 7.2, 7.5, 8.2, 8.3_

  - [x] 4.4 Add pagination controls component
    - Conditionally render only if Model.TotalPages > 1
    - Add Previous button with disabled state when Model.CurrentPage == 1
    - Add page indicator showing "Trang X / Y" format
    - Add Next button with disabled state when Model.CurrentPage == Model.TotalPages
    - Preserve category parameter in pagination links
    - _Requirements: 3.2, 3.3, 3.4, 3.5, 4.1, 4.2, 4.5, 8.1_

  - [ ]* 4.5 Write property test for pagination controls visibility
    - **Property 5: Pagination controls visibility**
    - **Validates: Requirements 3.2**

  - [ ]* 4.6 Write property test for pagination button boundary states
    - **Property 6: Pagination button boundary states**
    - **Validates: Requirements 3.4, 3.5**

  - [ ]* 4.7 Write property test for active category highlighting
    - **Property 8: Active category highlighting**
    - **Validates: Requirements 1.5**

- [x] 5. Add custom CSS for sidebar and pagination styling
  - [x] 5.1 Create or update wwwroot/css/site.css with sidebar styles
    - Add .category-sidebar styles (width, sticky positioning, scrolling)
    - Add sidebar list and link styles
    - Add active state styling
    - Add hover effects
    - _Requirements: 1.1, 1.5_

  - [x] 5.2 Add mobile sidebar styles
    - Add mobile breakpoint styles (< 768px)
    - Implement fixed positioning with slide-in animation
    - Add .show class for visibility toggle
    - Add sidebar overlay styles
    - _Requirements: 6.1, 6.2, 6.3_

  - [x] 5.3 Add pagination control styles
    - Add .pagination-controls flexbox layout
    - Add button spacing and disabled state styles
    - Ensure responsive layout on all screen sizes
    - _Requirements: 6.4_

- [x] 6. Add JavaScript for mobile sidebar toggle
  - [x] 6.1 Implement sidebar toggle functionality
    - Add event listener to toggle button
    - Toggle .show class on sidebar
    - Add overlay element and toggle its visibility
    - _Requirements: 6.2_

  - [x] 6.2 Implement auto-close on category selection
    - Add event listeners to category links
    - Close sidebar on mobile when category is clicked
    - Check window width to only apply on mobile
    - _Requirements: 6.3_

  - [x] 6.3 Add scroll-to-top on page navigation
    - Scroll to product grid top when pagination links are clicked
    - _Requirements: 4.3_

  - [ ]* 6.4 Write E2E test for category filtering
    - Test clicking category updates URL and products
    - Verify all displayed products match selected category
    - _Requirements: 2.1, 2.3, 2.4_

  - [ ]* 6.5 Write E2E test for pagination navigation
    - Test clicking Next/Previous buttons updates page
    - Verify URL updates with page parameter
    - Verify page indicator updates correctly
    - _Requirements: 4.1, 4.2, 4.3_

  - [ ]* 6.6 Write E2E test for mobile sidebar toggle
    - Test sidebar toggle button shows/hides sidebar on mobile
    - Test sidebar auto-closes when category is selected
    - _Requirements: 6.2, 6.3_

  - [ ]* 6.7 Write E2E test for responsive layout
    - Test sidebar visibility at different breakpoints
    - Test pagination controls remain functional on all screen sizes
    - _Requirements: 6.1, 6.4, 6.5_

- [x] 7. Checkpoint - Ensure all tests pass
  - Ensure all tests pass, ask the user if questions arise.

- [x] 8. Add database indexes for performance
  - [x] 8.1 Create migration for Category and IsActive indexes
    - Add index on Products.Category column
    - Add composite index on Products.IsActive and Products.Category
    - Run migration to update database
    - _Performance optimization for filtering queries_

  - [ ]* 8.2 Write performance test for pagination queries
    - Test query execution time with 1000+ products
    - Verify pagination performance remains under 100ms
    - _Performance validation_

- [x] 9. Final checkpoint - Verify all requirements
  - Test complete user flow: browse categories, navigate pages, test mobile
  - Verify all existing functionality preserved (cart, product details, etc.)
  - Ensure all tests pass, ask the user if questions arise.

## Notes

- Tasks marked with `*` are optional and can be skipped for faster MVP
- Each task references specific requirements for traceability
- Property tests use FsCheck framework with minimum 100 iterations
- E2E tests use Playwright (already configured in project)
- Unit tests use xUnit with in-memory database
- The implementation maintains Bootstrap 5.3 styling consistency
- All query parameters use kebab-case for URL friendliness
