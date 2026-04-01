# Requirements Document

## Introduction

This document specifies requirements for implementing category-based filtering and pagination for the Menu page in the SaleStore coffee shop application. The feature will allow users to browse products by category using a sidebar navigation and view products in paginated sets, improving usability when the product catalog grows.

## Glossary

- **Menu_Page**: The Razor Page at `/Pages/Menu.cshtml` that displays the product catalog
- **Product**: A sellable item with properties including Id, Name, Description, Price, Category, ImageUrl, Stock, IsActive
- **Category**: A classification grouping for products (e.g., "Cà phê", "Trà", "Bánh")
- **Sidebar**: A vertical navigation component on the left side of the Menu_Page displaying category filters
- **Product_Grid**: The main content area displaying products in a responsive grid layout
- **Page_Size**: The number of products displayed per page (default: 12)
- **Active_Category**: The currently selected category filter
- **Pagination_Controls**: UI elements (Previous, Next, page numbers) for navigating between pages

## Requirements

### Requirement 1: Display Category Sidebar

**User Story:** As a customer, I want to see a list of product categories in a sidebar, so that I can quickly navigate to products I'm interested in.

#### Acceptance Criteria

1. THE Menu_Page SHALL display a Sidebar on the left side of the Product_Grid
2. THE Sidebar SHALL list all unique categories from active products in the database
3. THE Sidebar SHALL include a "Tất cả" (All) option at the top of the category list
4. THE Sidebar SHALL display the count of products for each category in parentheses
5. THE Sidebar SHALL highlight the Active_Category with distinct styling
6. WHEN a category has zero products, THE Sidebar SHALL NOT display that category

### Requirement 2: Filter Products by Category

**User Story:** As a customer, I want to click on a category in the sidebar, so that I can view only products from that category.

#### Acceptance Criteria

1. WHEN a user clicks a category in the Sidebar, THE Menu_Page SHALL reload and display only products matching that category
2. WHEN a user clicks "Tất cả" (All), THE Menu_Page SHALL display products from all categories
3. THE Menu_Page SHALL preserve the selected category in the URL query string as a parameter named "category"
4. WHEN the Menu_Page loads with a category query parameter, THE Menu_Page SHALL filter products by that category
5. THE Menu_Page SHALL display only products where IsActive is true

### Requirement 3: Implement Pagination

**User Story:** As a customer, I want products to be displayed in pages, so that the page loads quickly and I can browse through products systematically.

#### Acceptance Criteria

1. THE Menu_Page SHALL display a maximum of Page_Size products per page
2. WHEN the filtered product count exceeds Page_Size, THE Menu_Page SHALL display Pagination_Controls below the Product_Grid
3. THE Pagination_Controls SHALL include Previous button, Next button, and current page indicator
4. WHEN a user is on the first page, THE Pagination_Controls SHALL disable the Previous button
5. WHEN a user is on the last page, THE Pagination_Controls SHALL disable the Next button
6. THE Menu_Page SHALL preserve the current page number in the URL query string as a parameter named "page"

### Requirement 4: Navigate Between Pages

**User Story:** As a customer, I want to click pagination controls, so that I can view different sets of products.

#### Acceptance Criteria

1. WHEN a user clicks the Next button, THE Menu_Page SHALL navigate to the next page and display the next set of products
2. WHEN a user clicks the Previous button, THE Menu_Page SHALL navigate to the previous page and display the previous set of products
3. WHEN a user navigates to a new page, THE Menu_Page SHALL scroll to the top of the Product_Grid
4. WHEN a user changes the Active_Category, THE Menu_Page SHALL reset to page 1
5. THE Menu_Page SHALL preserve both category and page parameters in the URL simultaneously

### Requirement 5: Handle Invalid Pagination Parameters

**User Story:** As a customer, I want the application to handle invalid page numbers gracefully, so that I don't encounter errors when browsing.

#### Acceptance Criteria

1. WHEN the page parameter is less than 1, THE Menu_Page SHALL redirect to page 1
2. WHEN the page parameter exceeds the total number of pages, THE Menu_Page SHALL redirect to the last valid page
3. WHEN the page parameter is not a valid integer, THE Menu_Page SHALL default to page 1
4. WHEN the category parameter does not match any existing category, THE Menu_Page SHALL display all products

### Requirement 6: Maintain Responsive Layout

**User Story:** As a customer using a mobile device, I want the category sidebar and pagination to work well on small screens, so that I can browse products comfortably.

#### Acceptance Criteria

1. WHEN the viewport width is less than 768px, THE Sidebar SHALL be hidden by default
2. WHEN the viewport width is less than 768px, THE Menu_Page SHALL display a category filter button that toggles the Sidebar visibility
3. WHEN a mobile user selects a category, THE Sidebar SHALL automatically close
4. THE Pagination_Controls SHALL remain visible and functional on all screen sizes
5. THE Product_Grid SHALL maintain its responsive column layout (1 column on mobile, 2 on tablet, 3-4 on desktop)

### Requirement 7: Preserve Existing Functionality

**User Story:** As a customer, I want all existing menu features to continue working, so that my shopping experience is not disrupted.

#### Acceptance Criteria

1. THE Menu_Page SHALL continue to display product images, names, descriptions, prices, and stock status
2. THE "Đặt mua" (Add to Cart) button SHALL continue to function for all displayed products
3. THE Menu_Page SHALL continue to use Tailwind CSS for styling consistency
4. THE Menu_Page SHALL continue to integrate with the existing Navbar and CartDrawer ViewComponents
5. THE Menu_Page SHALL continue to link to product detail pages when product cards are clicked

### Requirement 8: Display Pagination Information

**User Story:** As a customer, I want to see which page I'm on and how many pages exist, so that I understand my position in the product catalog.

#### Acceptance Criteria

1. THE Pagination_Controls SHALL display the current page number and total page count in the format "Trang X / Y"
2. THE Menu_Page SHALL display the total count of filtered products above the Product_Grid
3. WHEN no products match the filter, THE Menu_Page SHALL display a message "Không tìm thấy sản phẩm" (No products found)
4. WHEN displaying the last page with fewer than Page_Size products, THE Menu_Page SHALL display only the available products without empty placeholders
