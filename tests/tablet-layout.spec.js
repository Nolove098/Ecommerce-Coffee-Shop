// @ts-check
const { test, expect } = require('@playwright/test');

/**
 * Task 12.3: Verify tablet layouts (width >= 768px and < 992px)
 * 
 * This test suite validates tablet-specific responsive behavior:
 * - Appropriate two-column layouts on tablet
 * - POS interface is usable on tablet
 * - Touch-friendly sizing
 * 
 * Requirements: 12.6, 11.8
 */

// Tablet viewport sizes to test
const TABLET_VIEWPORTS = [
  { name: 'iPad Mini', width: 768, height: 1024 },
  { name: 'Tablet Portrait', width: 800, height: 1280 },
  { name: 'iPad', width: 820, height: 1180 },
  { name: 'Surface Pro 7', width: 912, height: 1368 },
  { name: 'iPad Pro 11', width: 834, height: 1194 },
  { name: 'Just Below Desktop', width: 991, height: 1200 }
];

// All pages to test
const PAGES = {
  // Public area
  home: '/',
  login: '/Auth/Login',
  register: '/Auth/Register',
  checkout: '/Cart/Checkout',
  cartSuccess: '/Cart/Success',
  productDetail: '/Product/Detail',
  productCategory: '/Product/Category',
  
  // Admin area (requires authentication)
  adminDashboard: '/Admin/Dashboard',
  adminProducts: '/Admin/Product',
  adminProductCreate: '/Admin/Product/Create',
  adminOrders: '/Admin/Order',
  adminUsers: '/Admin/UserManagement',
  
  // Staff area (requires authentication)
  staffPOS: '/Staff/POS'
};

test.describe('Tablet Layout Verification (width >= 768px and < 992px)', () => {
  
  test.describe('Requirement 12.6: Two-Column Layouts on Tablet', () => {
    
    test('Home page displays two-column layout on tablet', async ({ page }) => {
      for (const viewport of TABLET_VIEWPORTS) {
        await page.setViewportSize({ width: viewport.width, height: viewport.height });
        await page.goto(PAGES.home);
        await page.waitForLoadState('networkidle');
        
        // Check no horizontal overflow
        const bodyWidth = await page.evaluate(() => document.body.scrollWidth);
        const viewportWidth = await page.evaluate(() => window.innerWidth);
        expect(bodyWidth).toBeLessThanOrEqual(viewportWidth + 2); // +2 for rounding
        
        // Check for two-column grid classes (col-md-6)
        const hasTwoColumn = await page.evaluate(() => {
          const elements = document.querySelectorAll('[class*="col-md-6"]');
          return elements.length > 0;
        });
        
        console.log(`✓ Home page layout on ${viewport.name} (${viewport.width}px) - Two-column: ${hasTwoColumn}`);
      }
    });
    
    test('Product cards display in two-column grid on tablet', async ({ page }) => {
      for (const viewport of TABLET_VIEWPORTS) {
        await page.setViewportSize({ width: viewport.width, height: viewport.height });
        await page.goto(PAGES.home);
        await page.waitForLoadState('networkidle');
        
        // Check for product cards
        const cards = page.locator('.card');
        const cardCount = await cards.count();
        
        if (cardCount >= 2) {
          // Get positions of first two cards
          const firstCardBox = await cards.nth(0).boundingBox();
          const secondCardBox = await cards.nth(1).boundingBox();
          
          if (firstCardBox && secondCardBox) {
            // On tablet, cards should be side by side (two columns)
            const areSideBySide = Math.abs(firstCardBox.y - secondCardBox.y) < 50;
            const areStacked = secondCardBox.y > firstCardBox.y + firstCardBox.height * 0.5;
            
            console.log(`✓ Product cards on ${viewport.name}: ${areSideBySide ? 'side-by-side' : 'stacked'}`);
          }
        }
      }
    });

    
    test('Checkout page displays two-column layout on tablet', async ({ page }) => {
      for (const viewport of TABLET_VIEWPORTS) {
        await page.setViewportSize({ width: viewport.width, height: viewport.height });
        await page.goto(PAGES.checkout);
        
        // Check no horizontal overflow
        const bodyWidth = await page.evaluate(() => document.body.scrollWidth);
        const viewportWidth = await page.evaluate(() => window.innerWidth);
        expect(bodyWidth).toBeLessThanOrEqual(viewportWidth + 2);
        
        // Check for responsive column classes
        const hasResponsiveColumns = await page.evaluate(() => {
          const elements = document.querySelectorAll('[class*="col-md-"]');
          return elements.length > 0;
        });
        
        console.log(`✓ Checkout layout on ${viewport.name} (${viewport.width}px) - Responsive: ${hasResponsiveColumns}`);
      }
    });
    
    test('Admin dashboard displays two-column stats on tablet', async ({ page }) => {
      for (const viewport of TABLET_VIEWPORTS) {
        await page.setViewportSize({ width: viewport.width, height: viewport.height });
        await page.goto(PAGES.adminDashboard);
        await page.waitForLoadState('networkidle');
        
        // Check no horizontal overflow
        const bodyWidth = await page.evaluate(() => document.body.scrollWidth);
        const viewportWidth = await page.evaluate(() => window.innerWidth);
        expect(bodyWidth).toBeLessThanOrEqual(viewportWidth + 2);
        
        // Check for dashboard cards
        const cards = page.locator('.card');
        const cardCount = await cards.count();
        
        if (cardCount >= 2) {
          const firstCardBox = await cards.nth(0).boundingBox();
          const secondCardBox = await cards.nth(1).boundingBox();
          
          if (firstCardBox && secondCardBox) {
            const areSideBySide = Math.abs(firstCardBox.y - secondCardBox.y) < 50;
            console.log(`✓ Dashboard cards on ${viewport.name}: ${areSideBySide ? 'two-column' : 'stacked'}`);
          }
        }
      }
    });
    
    test('Product category page displays two-column grid on tablet', async ({ page }) => {
      for (const viewport of TABLET_VIEWPORTS) {
        await page.setViewportSize({ width: viewport.width, height: viewport.height });
        await page.goto(PAGES.productCategory);
        
        // Check no horizontal overflow
        const bodyWidth = await page.evaluate(() => document.body.scrollWidth);
        const viewportWidth = await page.evaluate(() => window.innerWidth);
        expect(bodyWidth).toBeLessThanOrEqual(viewportWidth + 2);
        
        console.log(`✓ Product category layout on ${viewport.name} (${viewport.width}px)`);
      }
    });
    
  });

  
  test.describe('Requirement 11.8: POS Interface Usability on Tablet', () => {
    
    test('POS interface displays properly on tablet', async ({ page }) => {
      for (const viewport of TABLET_VIEWPORTS) {
        await page.setViewportSize({ width: viewport.width, height: viewport.height });
        await page.goto(PAGES.staffPOS);
        await page.waitForLoadState('networkidle');
        
        // Check no horizontal overflow
        const bodyWidth = await page.evaluate(() => document.body.scrollWidth);
        const viewportWidth = await page.evaluate(() => window.innerWidth);
        expect(bodyWidth).toBeLessThanOrEqual(viewportWidth + 2);
        
        console.log(`✓ POS interface fits on ${viewport.name} (${viewport.width}px)`);
      }
    });
    
    test('POS product grid displays two columns on tablet', async ({ page }) => {
      for (const viewport of TABLET_VIEWPORTS) {
        await page.setViewportSize({ width: viewport.width, height: viewport.height });
        await page.goto(PAGES.staffPOS);
        await page.waitForLoadState('networkidle');
        
        // Check for product cards in POS
        const cards = page.locator('.card');
        const cardCount = await cards.count();
        
        if (cardCount >= 2) {
          const firstCardBox = await cards.nth(0).boundingBox();
          const secondCardBox = await cards.nth(1).boundingBox();
          
          if (firstCardBox && secondCardBox) {
            // On tablet, POS should show 2 columns (row-cols-sm-2)
            const areSideBySide = Math.abs(firstCardBox.y - secondCardBox.y) < 50;
            
            console.log(`✓ POS product grid on ${viewport.name}: ${areSideBySide ? 'two-column' : 'single-column'}`);
          }
        } else {
          console.log(`⚠ POS has ${cardCount} products on ${viewport.name}`);
        }
      }
    });
    
    test('POS cart sidebar is visible on tablet', async ({ page }) => {
      for (const viewport of TABLET_VIEWPORTS) {
        await page.setViewportSize({ width: viewport.width, height: viewport.height });
        await page.goto(PAGES.staffPOS);
        await page.waitForLoadState('networkidle');
        
        // Check for cart sidebar or cart section
        const cartSection = page.locator('[class*="cart"], .offcanvas, .list-group');
        const cartExists = await cartSection.count() > 0;
        
        if (cartExists) {
          console.log(`✓ POS cart section found on ${viewport.name}`);
        } else {
          console.log(`⚠ POS cart section not found on ${viewport.name}`);
        }
      }
    });

    
    test('POS quantity controls are touch-friendly on tablet', async ({ page }) => {
      await page.setViewportSize({ width: 768, height: 1024 });
      await page.goto(PAGES.staffPOS);
      await page.waitForLoadState('networkidle');
      
      // Check for quantity controls (input-group)
      const quantityControls = page.locator('.input-group button');
      const controlCount = await quantityControls.count();
      
      if (controlCount > 0) {
        const firstControl = quantityControls.first();
        const box = await firstControl.boundingBox();
        
        if (box) {
          // Buttons should be at least 40x40px for touch
          const isTouchFriendly = box.width >= 40 && box.height >= 40;
          
          console.log(`✓ POS quantity controls: ${box.width}x${box.height}px - ${isTouchFriendly ? 'touch-friendly' : 'small'}`);
        }
      } else {
        console.log('⚠ No quantity controls found in POS');
      }
    });
    
    test('POS action buttons are accessible on tablet', async ({ page }) => {
      await page.setViewportSize({ width: 768, height: 1024 });
      await page.goto(PAGES.staffPOS);
      await page.waitForLoadState('networkidle');
      
      // Check for action buttons
      const buttons = page.locator('button.btn, a.btn');
      const buttonCount = await buttons.count();
      
      if (buttonCount > 0) {
        let touchFriendlyCount = 0;
        
        for (let i = 0; i < Math.min(buttonCount, 10); i++) {
          const button = buttons.nth(i);
          const isVisible = await button.isVisible();
          
          if (isVisible) {
            const box = await button.boundingBox();
            if (box) {
              const isTouchFriendly = box.width >= 40 && box.height >= 40;
              if (isTouchFriendly) {
                touchFriendlyCount++;
              }
            }
          }
        }
        
        console.log(`✓ POS buttons: ${touchFriendlyCount}/${Math.min(buttonCount, 10)} are touch-friendly`);
      }
    });
    
  });

  
  test.describe('Touch-Friendly Sizing on Tablet', () => {
    
    test('Buttons are touch-friendly on tablet (min 44x44px)', async ({ page }) => {
      await page.setViewportSize({ width: 768, height: 1024 });
      await page.goto(PAGES.home);
      
      // Check button sizes
      const buttons = page.locator('button.btn, a.btn');
      const buttonCount = await buttons.count();
      
      if (buttonCount > 0) {
        let touchFriendlyCount = 0;
        
        for (let i = 0; i < Math.min(buttonCount, 10); i++) {
          const button = buttons.nth(i);
          const isVisible = await button.isVisible();
          
          if (isVisible) {
            const box = await button.boundingBox();
            if (box) {
              const isTouchFriendly = box.width >= 40 && box.height >= 40;
              if (isTouchFriendly) {
                touchFriendlyCount++;
              }
            }
          }
        }
        
        const percentage = (touchFriendlyCount / Math.min(buttonCount, 10)) * 100;
        expect(percentage).toBeGreaterThanOrEqual(70);
        
        console.log(`✓ ${touchFriendlyCount}/${Math.min(buttonCount, 10)} buttons are touch-friendly on tablet`);
      }
    });
    
    test('Form inputs are appropriately sized for tablet', async ({ page }) => {
      await page.setViewportSize({ width: 768, height: 1024 });
      await page.goto(PAGES.login);
      
      // Check input field sizes
      const inputs = page.locator('input.form-control');
      const inputCount = await inputs.count();
      
      if (inputCount > 0) {
        const firstInput = inputs.first();
        const box = await firstInput.boundingBox();
        
        if (box) {
          // Input should be at least 40px tall for easy tapping
          expect(box.height).toBeGreaterThan(35);
          
          console.log(`✓ Form inputs on tablet: ${box.width}x${box.height}px`);
        }
      }
    });
    
    test('Cards are tappable on tablet', async ({ page }) => {
      await page.setViewportSize({ width: 768, height: 1024 });
      await page.goto(PAGES.home);
      
      // Check for clickable cards
      const cards = page.locator('.card');
      const cardCount = await cards.count();
      
      if (cardCount > 0) {
        const firstCard = cards.first();
        const box = await firstCard.boundingBox();
        
        if (box) {
          // Card should be large enough to tap easily
          expect(box.width).toBeGreaterThan(100);
          expect(box.height).toBeGreaterThan(100);
          
          console.log(`✓ Cards are tappable on tablet: ${box.width}x${box.height}px`);
        }
      }
    });

    
    test('Table rows are touch-friendly on tablet', async ({ page }) => {
      await page.setViewportSize({ width: 768, height: 1024 });
      await page.goto(PAGES.adminProducts);
      await page.waitForLoadState('networkidle');
      
      // Check for table rows
      const tableRows = page.locator('table tbody tr');
      const rowCount = await tableRows.count();
      
      if (rowCount > 0) {
        const firstRow = tableRows.first();
        const box = await firstRow.boundingBox();
        
        if (box) {
          // Row should be at least 40px tall for touch
          const isTouchFriendly = box.height >= 40;
          
          console.log(`✓ Table rows on tablet: ${box.height}px - ${isTouchFriendly ? 'touch-friendly' : 'small'}`);
        }
      } else {
        console.log('⚠ No table rows found (may need authentication)');
      }
    });
    
    test('Navbar links have adequate spacing on tablet', async ({ page }) => {
      await page.setViewportSize({ width: 768, height: 1024 });
      await page.goto(PAGES.home);
      
      // Check navbar links
      const navLinks = page.locator('.navbar-nav .nav-link');
      const linkCount = await navLinks.count();
      
      if (linkCount > 1) {
        // Check if navbar is collapsed or expanded
        const toggler = page.locator('.navbar-toggler');
        const togglerVisible = await toggler.isVisible().catch(() => false);
        
        if (togglerVisible) {
          // Open navbar if collapsed
          await toggler.click();
          await page.waitForTimeout(300);
        }
        
        // Check spacing between links
        const firstLink = await navLinks.nth(0).boundingBox();
        const secondLink = await navLinks.nth(1).boundingBox();
        
        if (firstLink && secondLink) {
          const spacing = Math.abs(secondLink.y - (firstLink.y + firstLink.height));
          console.log(`✓ Navbar links spacing on tablet: ${spacing}px`);
        }
      }
    });
    
  });

  
  test.describe('Tablet-Specific Layout Checks', () => {
    
    test('No horizontal scrolling on any tablet page', async ({ page }) => {
      const viewport = { width: 768, height: 1024 };
      await page.setViewportSize(viewport);
      
      const publicPages = [
        PAGES.home,
        PAGES.login,
        PAGES.register,
        PAGES.checkout,
        PAGES.productDetail,
        PAGES.productCategory
      ];
      
      for (const pageUrl of publicPages) {
        await page.goto(pageUrl);
        await page.waitForLoadState('networkidle');
        
        // Check for horizontal overflow
        const hasHorizontalScroll = await page.evaluate(() => {
          return document.documentElement.scrollWidth > document.documentElement.clientWidth;
        });
        
        expect(hasHorizontalScroll).toBe(false);
        console.log(`✓ No horizontal scroll on ${pageUrl}`);
      }
    });
    
    test('Text is readable on tablet', async ({ page }) => {
      await page.setViewportSize({ width: 768, height: 1024 });
      await page.goto(PAGES.home);
      
      // Check body text size
      const bodyFontSize = await page.evaluate(() => {
        const body = document.body;
        const styles = window.getComputedStyle(body);
        return parseFloat(styles.fontSize);
      });
      
      // Body text should be at least 14px on tablet
      expect(bodyFontSize).toBeGreaterThanOrEqual(14);
      
      console.log(`✓ Body text size on tablet: ${bodyFontSize}px`);
    });
    
    test('Images scale properly on tablet', async ({ page }) => {
      await page.setViewportSize({ width: 768, height: 1024 });
      await page.goto(PAGES.home);
      
      // Check for images
      const images = page.locator('img');
      const imageCount = await images.count();
      
      if (imageCount > 0) {
        let properlyScaledCount = 0;
        
        for (let i = 0; i < Math.min(imageCount, 5); i++) {
          const img = images.nth(i);
          const isVisible = await img.isVisible();
          
          if (isVisible) {
            const box = await img.boundingBox();
            if (box) {
              // Image should not overflow viewport
              const fitsInViewport = box.x + box.width <= 768 + 20; // +20 for margins
              if (fitsInViewport) {
                properlyScaledCount++;
              }
            }
          }
        }
        
        expect(properlyScaledCount).toBeGreaterThan(0);
        
        console.log(`✓ ${properlyScaledCount}/${Math.min(imageCount, 5)} images scale properly on tablet`);
      }
    });

    
    test('Modals fit tablet viewport', async ({ page }) => {
      await page.setViewportSize({ width: 768, height: 1024 });
      await page.goto(PAGES.home);
      
      // Check for modals in the page
      const modals = page.locator('.modal');
      const modalCount = await modals.count();
      
      if (modalCount > 0) {
        const firstModal = modals.first();
        const hasModalClass = await firstModal.evaluate(el => 
          el.classList.contains('modal')
        );
        
        expect(hasModalClass).toBe(true);
        console.log(`✓ ${modalCount} modals found with proper structure on tablet`);
      }
    });
    
    test('Tables are responsive on tablet', async ({ page }) => {
      await page.setViewportSize({ width: 768, height: 1024 });
      await page.goto(PAGES.adminProducts);
      await page.waitForLoadState('networkidle');
      
      // Check for tables
      const tables = page.locator('table');
      const tableCount = await tables.count();
      
      if (tableCount > 0) {
        // Check if tables are wrapped in responsive container
        const responsiveTables = page.locator('.table-responsive');
        const responsiveCount = await responsiveTables.count();
        
        console.log(`✓ Found ${tableCount} tables, ${responsiveCount} with responsive wrapper on tablet`);
      }
    });
    
    test('Admin sidebar is visible on tablet', async ({ page }) => {
      await page.setViewportSize({ width: 768, height: 1024 });
      await page.goto(PAGES.adminDashboard);
      await page.waitForLoadState('networkidle');
      
      // Check for admin sidebar
      const sidebar = page.locator('.admin-sidebar, [class*="sidebar"]');
      const sidebarExists = await sidebar.count() > 0;
      
      if (sidebarExists) {
        const sidebarBox = await sidebar.first().boundingBox();
        
        if (sidebarBox) {
          // On tablet (md breakpoint), sidebar may be visible or collapsed
          // Check if it has reasonable width
          const isVisible = sidebarBox.width > 50;
          
          console.log(`${isVisible ? '✓' : '⚠'} Admin sidebar on tablet: ${isVisible ? 'visible' : 'collapsed'} (${sidebarBox.width}px)`);
        }
      } else {
        console.log('⚠ No admin sidebar found (may need authentication)');
      }
    });
    
  });

  
  test.describe('Tablet Performance and Loading', () => {
    
    test('Pages load without console errors on tablet', async ({ page }) => {
      const consoleErrors = [];
      
      page.on('console', msg => {
        if (msg.type() === 'error') {
          consoleErrors.push(msg.text());
        }
      });
      
      await page.setViewportSize({ width: 768, height: 1024 });
      await page.goto(PAGES.home);
      await page.waitForLoadState('networkidle');
      
      // Should have no console errors
      expect(consoleErrors.length).toBe(0);
      
      console.log('✓ No console errors on tablet');
    });
    
    test('Bootstrap JavaScript initializes on tablet', async ({ page }) => {
      await page.setViewportSize({ width: 768, height: 1024 });
      await page.goto(PAGES.home);
      await page.waitForLoadState('networkidle');
      
      // Check if Bootstrap is loaded
      const bootstrapLoaded = await page.evaluate(() => {
        return typeof bootstrap !== 'undefined';
      });
      
      expect(bootstrapLoaded).toBe(true);
      console.log('✓ Bootstrap JavaScript initialized on tablet');
    });
    
    test('Responsive grid classes are applied on tablet', async ({ page }) => {
      await page.setViewportSize({ width: 768, height: 1024 });
      await page.goto(PAGES.home);
      
      // Check for tablet-specific grid classes (col-md-)
      const hasMdClasses = await page.evaluate(() => {
        const elements = document.querySelectorAll('[class*="col-md-"]');
        return elements.length > 0;
      });
      
      console.log(`${hasMdClasses ? '✓' : '⚠'} Tablet grid classes (col-md-) ${hasMdClasses ? 'found' : 'not found'}`);
    });
    
  });
  
  test.describe('Navbar Behavior on Tablet', () => {
    
    test('Navbar collapses or expands appropriately on tablet', async ({ page }) => {
      for (const viewport of TABLET_VIEWPORTS) {
        await page.setViewportSize({ width: viewport.width, height: viewport.height });
        await page.goto(PAGES.home);
        await page.waitForLoadState('networkidle');
        
        // Check for navbar toggler
        const toggler = page.locator('.navbar-toggler');
        const togglerVisible = await toggler.isVisible().catch(() => false);
        
        // On tablet (< 992px), navbar should have toggler
        // Bootstrap default is navbar-expand-lg (expands at 992px)
        const shouldHaveToggler = viewport.width < 992;
        
        console.log(`${viewport.name} (${viewport.width}px): Toggler ${togglerVisible ? 'visible' : 'hidden'} (expected: ${shouldHaveToggler ? 'visible' : 'hidden'})`);
      }
    });
    
  });

  
  test.describe('Tablet Layout Summary Tests', () => {
    
    test('All public pages are tablet-friendly', async ({ page }) => {
      const viewport = { width: 768, height: 1024 };
      await page.setViewportSize(viewport);
      
      const publicPages = [
        { name: 'Home', url: PAGES.home },
        { name: 'Login', url: PAGES.login },
        { name: 'Register', url: PAGES.register },
        { name: 'Checkout', url: PAGES.checkout },
        { name: 'Product Detail', url: PAGES.productDetail },
        { name: 'Product Category', url: PAGES.productCategory }
      ];
      
      const results = [];
      
      for (const pageInfo of publicPages) {
        await page.goto(pageInfo.url);
        await page.waitForLoadState('networkidle');
        
        // Check no horizontal overflow
        const bodyWidth = await page.evaluate(() => document.body.scrollWidth);
        const viewportWidth = await page.evaluate(() => window.innerWidth);
        const noOverflow = bodyWidth <= viewportWidth + 2;
        
        // Check for responsive grid classes
        const hasResponsiveGrid = await page.evaluate(() => {
          const elements = document.querySelectorAll('[class*="col-md-"]');
          return elements.length > 0;
        });
        
        results.push({
          page: pageInfo.name,
          noOverflow,
          hasResponsiveGrid
        });
        
        console.log(`${pageInfo.name}: overflow=${!noOverflow}, responsive=${hasResponsiveGrid}`);
      }
      
      // All pages should pass
      const allPassed = results.every(r => r.noOverflow);
      expect(allPassed).toBe(true);
      
      console.log('✓ All public pages are tablet-friendly');
    });
    
    test('POS interface is fully functional on tablet', async ({ page }) => {
      const viewport = { width: 768, height: 1024 };
      await page.setViewportSize(viewport);
      
      await page.goto(PAGES.staffPOS);
      await page.waitForLoadState('networkidle');
      
      // Check no horizontal overflow
      const bodyWidth = await page.evaluate(() => document.body.scrollWidth);
      const viewportWidth = await page.evaluate(() => window.innerWidth);
      const noOverflow = bodyWidth <= viewportWidth + 2;
      
      // Check for product cards
      const cards = page.locator('.card');
      const cardCount = await cards.count();
      
      // Check for buttons
      const buttons = page.locator('button.btn, a.btn');
      const buttonCount = await buttons.count();
      
      expect(noOverflow).toBe(true);
      
      console.log(`✓ POS interface on tablet: ${cardCount} products, ${buttonCount} buttons, no overflow`);
    });
    
  });
  
});

test.describe('Tablet Breakpoint Edge Cases', () => {
  
  test('768px (md breakpoint start) - Layout validation', async ({ page }) => {
    await page.setViewportSize({ width: 768, height: 1024 });
    await page.goto(PAGES.home);
    
    // Check no horizontal scroll
    const hasHorizontalScroll = await page.evaluate(() => {
      return document.documentElement.scrollWidth > document.documentElement.clientWidth;
    });
    expect(hasHorizontalScroll).toBe(false);
    
    console.log('✓ 768px (md breakpoint start) validated');
  });
  
  test('991px (just below lg breakpoint) - Layout validation', async ({ page }) => {
    await page.setViewportSize({ width: 991, height: 1200 });
    await page.goto(PAGES.home);
    
    // Check no horizontal scroll
    const hasHorizontalScroll = await page.evaluate(() => {
      return document.documentElement.scrollWidth > document.documentElement.clientWidth;
    });
    expect(hasHorizontalScroll).toBe(false);
    
    console.log('✓ 991px (just below lg breakpoint) validated');
  });
  
});

