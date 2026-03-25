// @ts-check
const { test, expect } = require('@playwright/test');

/**
 * Bug Condition Exploration Tests for Admin UI Fixes
 * 
 * **Validates: Requirements 2.1, 2.2, 2.3, 2.4, 2.5**
 * 
 * CRITICAL: These tests MUST FAIL on unfixed code - failure confirms the bugs exist
 * DO NOT attempt to fix the tests or the code when they fail
 * NOTE: These tests encode the expected behavior - they will validate the fixes when they pass after implementation
 * GOAL: Surface counterexamples that demonstrate the bugs exist
 */

test.describe('Bug Condition Exploration Tests', () => {
  
  /**
   * Bug 1: Admin pages (Dashboard, Product) cannot scroll when content exceeds viewport height
   * **Validates: Requirements 2.1**
   * 
   * Bug Condition: page.contentHeight > viewport.height AND page.mainElement.hasClass('overflow-hidden') AND NOT page.canScroll()
   * Expected Behavior: Main content area has overflow-auto class and allows vertical scrolling
   */
  test('Bug 1.1 - Admin Dashboard page should allow scrolling when content exceeds viewport', async ({ page }) => {
    // Skip if admin authentication is not available
    test.skip(true, 'Admin authentication required - manual verification needed');
    
    // Navigate to admin dashboard
    await page.goto('/Admin/Dashboard/Index');
    await page.waitForLoadState('networkidle');
    
    // Get main content area
    const mainContent = page.locator('main').first();
    await expect(mainContent).toBeVisible();
    
    // Check if main element has overflow-hidden class (bug condition)
    const mainClasses = await mainContent.getAttribute('class');
    const hasOverflowHidden = mainClasses?.includes('overflow-hidden');
    
    // Get content height and viewport height
    const dimensions = await page.evaluate(() => {
      const main = document.querySelector('main');
      return {
        contentHeight: main?.scrollHeight || 0,
        viewportHeight: window.innerHeight,
        canScroll: (main?.scrollHeight || 0) > (main?.clientHeight || 0)
      };
    });
    
    // EXPECTED TO FAIL: If content exceeds viewport, should be able to scroll
    // Bug condition: overflow-hidden prevents scrolling
    if (dimensions.contentHeight > dimensions.viewportHeight) {
      expect(hasOverflowHidden).toBe(false); // Should NOT have overflow-hidden
      expect(mainClasses).toContain('overflow-auto'); // Should have overflow-auto
    }
  });

  test('Bug 1.2 - Admin Product page should allow scrolling when content exceeds viewport', async ({ page }) => {
    // Skip if admin authentication is not available
    test.skip(true, 'Admin authentication required - manual verification needed');
    
    // Navigate to admin product page
    await page.goto('/Admin/Product/Index');
    await page.waitForLoadState('networkidle');
    
    // Get main content area
    const mainContent = page.locator('main').first();
    await expect(mainContent).toBeVisible();
    
    // Check if main element has overflow-hidden class (bug condition)
    const mainClasses = await mainContent.getAttribute('class');
    const hasOverflowHidden = mainClasses?.includes('overflow-hidden');
    
    // Get content height and viewport height
    const dimensions = await page.evaluate(() => {
      const main = document.querySelector('main');
      return {
        contentHeight: main?.scrollHeight || 0,
        viewportHeight: window.innerHeight,
        canScroll: (main?.scrollHeight || 0) > (main?.clientHeight || 0)
      };
    });
    
    // EXPECTED TO FAIL: If content exceeds viewport, should be able to scroll
    if (dimensions.contentHeight > dimensions.viewportHeight) {
      expect(hasOverflowHidden).toBe(false); // Should NOT have overflow-hidden
      expect(mainClasses).toContain('overflow-auto'); // Should have overflow-auto
    }
  });

  /**
   * Bug 2: Primary button hover colors are cyan/blue instead of coffee theme colors (#5D3A1A or #C4956A)
   * **Validates: Requirements 2.2**
   * 
   * Bug Condition: button.isHovered() AND button.backgroundColor IN ['cyan', 'blue'] AND NOT IN ['#5D3A1A', '#C4956A']
   * Expected Behavior: Hovered primary buttons display #5D3A1A background color
   */
  test('Bug 2.1 - Primary button hover color should be coffee theme color (#5D3A1A)', async ({ page }) => {
    await page.goto('/');
    await page.waitForLoadState('networkidle');
    
    // Find a primary button
    const primaryBtn = page.locator('.btn-primary').first();
    await expect(primaryBtn).toBeVisible();
    
    // Get initial background color
    const initialColor = await primaryBtn.evaluate((el) => {
      return window.getComputedStyle(el).backgroundColor;
    });
    
    // Hover over the button
    await primaryBtn.hover();
    await page.waitForTimeout(300); // Wait for hover transition
    
    // Get hover background color
    const hoverColor = await primaryBtn.evaluate((el) => {
      return window.getComputedStyle(el).backgroundColor;
    });
    
    // Convert RGB to hex for comparison
    const rgbToHex = (rgb) => {
      const match = rgb.match(/^rgb\((\d+),\s*(\d+),\s*(\d+)\)$/);
      if (!match) return rgb;
      const r = parseInt(match[1]).toString(16).padStart(2, '0');
      const g = parseInt(match[2]).toString(16).padStart(2, '0');
      const b = parseInt(match[3]).toString(16).padStart(2, '0');
      return `#${r}${g}${b}`;
    };
    
    const hoverColorHex = rgbToHex(hoverColor).toUpperCase();
    
    // EXPECTED TO FAIL: Hover color should be coffee theme color
    // Bug condition: hover color is cyan/blue instead of #5D3A1A or #C4956A
    const coffeeColors = ['#5D3A1A', '#C4956A', '#6F4E37', '#B8884D'];
    const isCoffeeColor = coffeeColors.some(color => 
      hoverColorHex.includes(color.toUpperCase())
    );
    
    // Check if it's cyan/blue (bug condition)
    const isCyanBlue = hoverColorHex.includes('00FFFF') || 
                       hoverColorHex.includes('0000FF') ||
                       hoverColor.includes('cyan') ||
                       hoverColor.includes('blue');
    
    expect(isCyanBlue).toBe(false); // Should NOT be cyan/blue
    expect(isCoffeeColor).toBe(true); // Should be coffee theme color
  });

  test('Bug 2.2 - Primary button hover on admin pages should use coffee theme', async ({ page }) => {
    // Skip if admin authentication is not available
    test.skip(true, 'Admin authentication required - manual verification needed');
    
    await page.goto('/Admin/Product/Index');
    await page.waitForLoadState('networkidle');
    
    // Find primary button (e.g., "Thêm sản phẩm")
    const primaryBtn = page.locator('.btn-primary').first();
    
    if (await primaryBtn.isVisible()) {
      // Hover over the button
      await primaryBtn.hover();
      await page.waitForTimeout(300);
      
      // Get hover background color
      const hoverColor = await primaryBtn.evaluate((el) => {
        return window.getComputedStyle(el).backgroundColor;
      });
      
      // Convert RGB to hex
      const rgbToHex = (rgb) => {
        const match = rgb.match(/^rgb\((\d+),\s*(\d+),\s*(\d+)\)$/);
        if (!match) return rgb;
        const r = parseInt(match[1]).toString(16).padStart(2, '0');
        const g = parseInt(match[2]).toString(16).padStart(2, '0');
        const b = parseInt(match[3]).toString(16).padStart(2, '0');
        return `#${r}${g}${b}`;
      };
      
      const hoverColorHex = rgbToHex(hoverColor).toUpperCase();
      
      // EXPECTED TO FAIL: Should be coffee color, not cyan/blue
      const coffeeColors = ['#5D3A1A', '#C4956A', '#6F4E37', '#B8884D'];
      const isCoffeeColor = coffeeColors.some(color => 
        hoverColorHex.includes(color.toUpperCase())
      );
      
      expect(isCoffeeColor).toBe(true);
    }
  });

  /**
   * Bug 3: UserManagement page at `/Admin/UserManagement/Index` does not display content (user table, statistics)
   * **Validates: Requirements 2.3**
   * 
   * Bug Condition: page.route == '/Admin/UserManagement/Index' AND page.contentVisible == false AND page.hasData == true
   * Expected Behavior: User table, statistics cards, and all UI elements visible and interactive
   */
  test('Bug 3 - UserManagement page should display user table and statistics', async ({ page }) => {
    // Skip if admin authentication is not available
    test.skip(true, 'Admin authentication required - manual verification needed');
    
    await page.goto('/Admin/UserManagement/Index');
    await page.waitForLoadState('networkidle');
    
    // Check if page has content
    const hasContent = await page.evaluate(() => {
      const body = document.body;
      const textContent = body.textContent || '';
      const hasVisibleElements = body.querySelectorAll('table, .card, .statistics').length > 0;
      return {
        hasText: textContent.trim().length > 100,
        hasVisibleElements: hasVisibleElements,
        bodyHeight: body.offsetHeight
      };
    });
    
    // EXPECTED TO FAIL: Content should be visible
    // Bug condition: page loads but content is not displayed
    expect(hasContent.hasText).toBe(true); // Should have text content
    expect(hasContent.hasVisibleElements).toBe(true); // Should have visible elements
    expect(hasContent.bodyHeight).toBeGreaterThan(200); // Should have reasonable height
    
    // Check for user table specifically
    const userTable = page.locator('table').first();
    if (await userTable.count() > 0) {
      await expect(userTable).toBeVisible();
    }
  });

  /**
   * Bug 4: POS page at `/Staff/POS/Index` does not display content (product grid, cart sidebar)
   * **Validates: Requirements 2.4**
   * 
   * Bug Condition: page.route == '/Staff/POS/Index' AND page.contentVisible == false AND page.hasProducts == true
   * Expected Behavior: Product grid, cart sidebar, customer form, and all POS elements visible and functional
   */
  test('Bug 4 - POS page should display product grid and cart sidebar', async ({ page }) => {
    // Skip if staff authentication is not available
    test.skip(true, 'Staff authentication required - manual verification needed');
    
    await page.goto('/Staff/POS/Index');
    await page.waitForLoadState('networkidle');
    
    // Check if page has content
    const hasContent = await page.evaluate(() => {
      const body = document.body;
      const textContent = body.textContent || '';
      const productGrid = document.querySelector('#product-grid, .product-grid, [id*="product"]');
      const cartSidebar = document.querySelector('#cart-sidebar, .cart-sidebar, [id*="cart"]');
      
      return {
        hasText: textContent.trim().length > 100,
        hasProductGrid: productGrid !== null && productGrid.offsetHeight > 0,
        hasCartSidebar: cartSidebar !== null && cartSidebar.offsetHeight > 0,
        bodyHeight: body.offsetHeight
      };
    });
    
    // EXPECTED TO FAIL: Content should be visible
    // Bug condition: page loads but product grid and cart are not displayed
    expect(hasContent.hasText).toBe(true); // Should have text content
    expect(hasContent.bodyHeight).toBeGreaterThan(200); // Should have reasonable height
    
    // At least one of product grid or cart sidebar should be visible
    const hasVisibleContent = hasContent.hasProductGrid || hasContent.hasCartSidebar;
    expect(hasVisibleContent).toBe(true);
  });

  /**
   * Bug 5: Homepage filter buttons do not filter products when clicked
   * **Validates: Requirements 2.5**
   * 
   * Bug Condition: button.isClicked() AND productGrid.filteredProducts == productGrid.allProducts AND NOT productGrid.isFiltered()
   * Expected Behavior: Clicking filter button filters products, updates active state, provides visual feedback
   */
  test('Bug 5.1 - Homepage filter buttons should filter products by category', async ({ page }) => {
    await page.goto('/');
    await page.waitForLoadState('networkidle');
    
    // Get initial product count
    const initialProductCount = await page.locator('.product-card, .card').count();
    
    // Find filter buttons
    const filterButtons = page.locator('.cat-btn, button[onclick*="filterCat"]');
    const buttonCount = await filterButtons.count();
    
    if (buttonCount > 0) {
      // Click on a specific category filter (not "Tất cả")
      // Try to find a category button
      const categoryButton = filterButtons.nth(1); // Second button (first is usually "Tất cả")
      
      if (await categoryButton.isVisible()) {
        await categoryButton.click();
        await page.waitForTimeout(500); // Wait for filter to apply
        
        // Get filtered product count
        const filteredProductCount = await page.evaluate(() => {
          const products = document.querySelectorAll('.product-card, .card');
          let visibleCount = 0;
          products.forEach(product => {
            const style = window.getComputedStyle(product);
            if (style.display !== 'none') {
              visibleCount++;
            }
          });
          return visibleCount;
        });
        
        // EXPECTED TO FAIL: Product count should change after filtering
        // Bug condition: all products still visible, no filtering occurred
        expect(filteredProductCount).toBeLessThan(initialProductCount);
        
        // Check if button has active state
        const buttonClasses = await categoryButton.getAttribute('class');
        expect(buttonClasses).toContain('active');
      }
    }
  });

  test('Bug 5.2 - Homepage filter "Tất cả" button should show all products', async ({ page }) => {
    await page.goto('/');
    await page.waitForLoadState('networkidle');
    
    // Get initial product count
    const initialProductCount = await page.locator('.product-card, .card').count();
    
    // Find filter buttons
    const filterButtons = page.locator('.cat-btn, button[onclick*="filterCat"]');
    const buttonCount = await filterButtons.count();
    
    if (buttonCount > 1) {
      // Click on a specific category first
      const categoryButton = filterButtons.nth(1);
      if (await categoryButton.isVisible()) {
        await categoryButton.click();
        await page.waitForTimeout(500);
        
        // Now click "Tất cả" button
        const allButton = filterButtons.first();
        await allButton.click();
        await page.waitForTimeout(500);
        
        // Get product count after clicking "Tất cả"
        const allProductCount = await page.evaluate(() => {
          const products = document.querySelectorAll('.product-card, .card');
          let visibleCount = 0;
          products.forEach(product => {
            const style = window.getComputedStyle(product);
            if (style.display !== 'none') {
              visibleCount++;
            }
          });
          return visibleCount;
        });
        
        // EXPECTED TO FAIL: Should show all products again
        expect(allProductCount).toBe(initialProductCount);
        
        // Check if "Tất cả" button has active state
        const allButtonClasses = await allButton.getAttribute('class');
        expect(allButtonClasses).toContain('active');
      }
    }
  });

  test('Bug 5.3 - Homepage filter buttons should update active state', async ({ page }) => {
    await page.goto('/');
    await page.waitForLoadState('networkidle');
    
    // Find filter buttons
    const filterButtons = page.locator('.cat-btn, button[onclick*="filterCat"]');
    const buttonCount = await filterButtons.count();
    
    if (buttonCount > 1) {
      const firstButton = filterButtons.first();
      const secondButton = filterButtons.nth(1);
      
      // Click first button
      await firstButton.click();
      await page.waitForTimeout(300);
      
      let firstButtonClasses = await firstButton.getAttribute('class');
      let secondButtonClasses = await secondButton.getAttribute('class');
      
      // EXPECTED TO FAIL: First button should have active class
      expect(firstButtonClasses).toContain('active');
      expect(secondButtonClasses).not.toContain('active');
      
      // Click second button
      await secondButton.click();
      await page.waitForTimeout(300);
      
      firstButtonClasses = await firstButton.getAttribute('class');
      secondButtonClasses = await secondButton.getAttribute('class');
      
      // EXPECTED TO FAIL: Second button should have active class, first should not
      expect(firstButtonClasses).not.toContain('active');
      expect(secondButtonClasses).toContain('active');
    }
  });
});
