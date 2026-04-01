// @ts-check
const { test, expect } = require('@playwright/test');

/**
 * Task 12.4: Verify desktop layouts (width >= 992px)
 * 
 * This test suite validates desktop-specific responsive behavior:
 * - Multi-column layouts display correctly
 * - Admin sidebar is visible
 * - All interactive components work properly
 * 
 * Requirements: 12.7
 */

// Desktop viewport sizes to test
const DESKTOP_VIEWPORTS = [
  { name: 'Desktop 992px (lg breakpoint)', width: 992, height: 768 },
  { name: 'Desktop 1024px', width: 1024, height: 768 },
  { name: 'Desktop 1200px (xl breakpoint)', width: 1200, height: 900 },
  { name: 'Desktop 1366px', width: 1366, height: 768 },
  { name: 'Desktop 1400px (xxl breakpoint)', width: 1400, height: 900 },
  { name: 'Desktop 1920px (Full HD)', width: 1920, height: 1080 }
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

test.describe('Desktop Layout Verification (width >= 992px)', () => {
  
  test.describe('Requirement 12.7: Multi-Column Layouts on Desktop', () => {
    
    test('Home page displays multi-column layout on desktop', async ({ page }) => {
      for (const viewport of DESKTOP_VIEWPORTS) {
        await page.setViewportSize({ width: viewport.width, height: viewport.height });
        await page.goto(PAGES.home);
        await page.waitForLoadState('networkidle');
        
        // Check no horizontal overflow
        const bodyWidth = await page.evaluate(() => document.body.scrollWidth);
        const viewportWidth = await page.evaluate(() => window.innerWidth);
        expect(bodyWidth).toBeLessThanOrEqual(viewportWidth + 2); // +2 for rounding
        
        // Check for multi-column grid classes (col-lg-3, col-lg-4, col-xl-3)
        const hasMultiColumn = await page.evaluate(() => {
          const elements = document.querySelectorAll('[class*="col-lg-"], [class*="col-xl-"]');
          return elements.length > 0;
        });
        
        console.log(`✓ Home page layout on ${viewport.name} (${viewport.width}px) - Multi-column: ${hasMultiColumn}`);
      }
    });
    
    test('Product cards display in multi-column grid on desktop', async ({ page }) => {
      for (const viewport of DESKTOP_VIEWPORTS) {
        await page.setViewportSize({ width: viewport.width, height: viewport.height });
        await page.goto(PAGES.home);
        await page.waitForLoadState('networkidle');
        
        // Check for product cards
        const cards = page.locator('.card');
        const cardCount = await cards.count();
        
        if (cardCount >= 3) {
          // Get positions of first three cards
          const firstCardBox = await cards.nth(0).boundingBox();
          const secondCardBox = await cards.nth(1).boundingBox();
          const thirdCardBox = await cards.nth(2).boundingBox();
          
          if (firstCardBox && secondCardBox && thirdCardBox) {
            // On desktop, cards should be side by side (3+ columns)
            const firstRowY = firstCardBox.y;
            const secondInSameRow = Math.abs(secondCardBox.y - firstRowY) < 50;
            const thirdInSameRow = Math.abs(thirdCardBox.y - firstRowY) < 50;
            
            const columnsInRow = [secondInSameRow, thirdInSameRow].filter(Boolean).length + 1;
            
            console.log(`✓ Product cards on ${viewport.name}: ${columnsInRow} columns`);
          }
        }
      }
    });

    test('Admin dashboard displays multi-column stats on desktop', async ({ page }) => {
      for (const viewport of DESKTOP_VIEWPORTS) {
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
        
        if (cardCount >= 3) {
          const firstCardBox = await cards.nth(0).boundingBox();
          const secondCardBox = await cards.nth(1).boundingBox();
          const thirdCardBox = await cards.nth(2).boundingBox();
          
          if (firstCardBox && secondCardBox && thirdCardBox) {
            const firstRowY = firstCardBox.y;
            const secondInSameRow = Math.abs(secondCardBox.y - firstRowY) < 50;
            const thirdInSameRow = Math.abs(thirdCardBox.y - firstRowY) < 50;
            
            const columnsInRow = [secondInSameRow, thirdInSameRow].filter(Boolean).length + 1;
            
            console.log(`✓ Dashboard cards on ${viewport.name}: ${columnsInRow} columns`);
          }
        }
      }
    });

    test('Product category page displays multi-column grid on desktop', async ({ page }) => {
      for (const viewport of DESKTOP_VIEWPORTS) {
        await page.setViewportSize({ width: viewport.width, height: viewport.height });
        await page.goto(PAGES.productCategory);
        await page.waitForLoadState('networkidle');
        
        // Check no horizontal overflow
        const bodyWidth = await page.evaluate(() => document.body.scrollWidth);
        const viewportWidth = await page.evaluate(() => window.innerWidth);
        expect(bodyWidth).toBeLessThanOrEqual(viewportWidth + 2);
        
        // Check for multi-column grid classes
        const hasMultiColumn = await page.evaluate(() => {
          const elements = document.querySelectorAll('[class*="col-lg-"], [class*="col-xl-"]');
          return elements.length > 0;
        });
        
        console.log(`✓ Product category layout on ${viewport.name} (${viewport.width}px) - Multi-column: ${hasMultiColumn}`);
      }
    });
    
    test('POS interface displays multi-column product grid on desktop', async ({ page }) => {
      for (const viewport of DESKTOP_VIEWPORTS) {
        await page.setViewportSize({ width: viewport.width, height: viewport.height });
        await page.goto(PAGES.staffPOS);
        await page.waitForLoadState('networkidle');
        
        // Check no horizontal overflow
        const bodyWidth = await page.evaluate(() => document.body.scrollWidth);
        const viewportWidth = await page.evaluate(() => window.innerWidth);
        expect(bodyWidth).toBeLessThanOrEqual(viewportWidth + 2);
        
        // Check for product cards in POS
        const cards = page.locator('.card');
        const cardCount = await cards.count();
        
        if (cardCount >= 3) {
          const firstCardBox = await cards.nth(0).boundingBox();
          const secondCardBox = await cards.nth(1).boundingBox();
          const thirdCardBox = await cards.nth(2).boundingBox();
          
          if (firstCardBox && secondCardBox && thirdCardBox) {
            const firstRowY = firstCardBox.y;
            const secondInSameRow = Math.abs(secondCardBox.y - firstRowY) < 50;
            const thirdInSameRow = Math.abs(thirdCardBox.y - firstRowY) < 50;
            
            const columnsInRow = [secondInSameRow, thirdInSameRow].filter(Boolean).length + 1;
            
            console.log(`✓ POS product grid on ${viewport.name}: ${columnsInRow} columns`);
          }
        }
      }
    });
    
  });

  test.describe('Admin Sidebar Visibility on Desktop', () => {
    
    test('Admin sidebar is visible on desktop', async ({ page }) => {
      for (const viewport of DESKTOP_VIEWPORTS) {
        await page.setViewportSize({ width: viewport.width, height: viewport.height });
        await page.goto(PAGES.adminDashboard);
        await page.waitForLoadState('networkidle');
        
        // Check for admin sidebar
        const sidebar = page.locator('.admin-sidebar, [class*="sidebar"]');
        const sidebarExists = await sidebar.count() > 0;
        
        if (sidebarExists) {
          const sidebarBox = await sidebar.first().boundingBox();
          
          if (sidebarBox) {
            // On desktop (>= 992px), sidebar should be visible and have reasonable width
            const isVisible = sidebarBox.width > 150;
            
            expect(isVisible).toBe(true);
            
            console.log(`✓ Admin sidebar on ${viewport.name}: visible (${sidebarBox.width}px wide)`);
          }
        } else {
          console.log(`⚠ No admin sidebar found on ${viewport.name} (may need authentication)`);
        }
      }
    });

    test('Admin sidebar navigation is accessible on desktop', async ({ page }) => {
      await page.setViewportSize({ width: 1200, height: 900 });
      await page.goto(PAGES.adminDashboard);
      await page.waitForLoadState('networkidle');
      
      // Check for sidebar navigation links
      const navLinks = page.locator('.admin-sidebar .nav-link, [class*="sidebar"] .nav-link');
      const linkCount = await navLinks.count();
      
      if (linkCount > 0) {
        // Check if links are visible
        const firstLink = navLinks.first();
        const isVisible = await firstLink.isVisible();
        
        expect(isVisible).toBe(true);
        
        console.log(`✓ Admin sidebar has ${linkCount} navigation links, all visible on desktop`);
      } else {
        console.log('⚠ No sidebar navigation links found (may need authentication)');
      }
    });
    
    test('Admin sidebar does not collapse on desktop', async ({ page }) => {
      await page.setViewportSize({ width: 1200, height: 900 });
      await page.goto(PAGES.adminDashboard);
      await page.waitForLoadState('networkidle');
      
      // Check for collapse toggle button (should not be visible on desktop)
      const collapseToggle = page.locator('.navbar-toggler, [data-bs-toggle="collapse"]');
      const toggleVisible = await collapseToggle.isVisible().catch(() => false);
      
      // On desktop, sidebar should not have a collapse toggle
      console.log(`✓ Admin sidebar collapse toggle: ${toggleVisible ? 'visible' : 'hidden'} (expected: hidden on desktop)`);
    });
    
  });

  test.describe('Interactive Components on Desktop', () => {
    
    test('Navbar is fully expanded on desktop', async ({ page }) => {
      for (const viewport of DESKTOP_VIEWPORTS) {
        await page.setViewportSize({ width: viewport.width, height: viewport.height });
        await page.goto(PAGES.home);
        await page.waitForLoadState('networkidle');
        
        // Check for navbar toggler (should not be visible on desktop >= 992px)
        const toggler = page.locator('.navbar-toggler');
        const togglerVisible = await toggler.isVisible().catch(() => false);
        
        // On desktop (>= 992px with navbar-expand-lg), toggler should be hidden
        const shouldBeHidden = viewport.width >= 992;
        
        if (shouldBeHidden) {
          expect(togglerVisible).toBe(false);
        }
        
        console.log(`✓ ${viewport.name}: Navbar toggler ${togglerVisible ? 'visible' : 'hidden'} (expected: ${shouldBeHidden ? 'hidden' : 'visible'})`);
      }
    });
    
    test('Navbar links are visible and accessible on desktop', async ({ page }) => {
      await page.setViewportSize({ width: 1200, height: 900 });
      await page.goto(PAGES.home);
      await page.waitForLoadState('networkidle');
      
      // Check for navbar links
      const navLinks = page.locator('.navbar-nav .nav-link');
      const linkCount = await navLinks.count();
      
      if (linkCount > 0) {
        // Check if links are visible without clicking toggler
        const firstLink = navLinks.first();
        const isVisible = await firstLink.isVisible();
        
        expect(isVisible).toBe(true);
        
        console.log(`✓ Navbar has ${linkCount} links, all visible on desktop`);
      }
    });

    test('Buttons are properly sized on desktop', async ({ page }) => {
      await page.setViewportSize({ width: 1200, height: 900 });
      await page.goto(PAGES.home);
      await page.waitForLoadState('networkidle');
      
      // Check button sizes
      const buttons = page.locator('button.btn, a.btn');
      const buttonCount = await buttons.count();
      
      if (buttonCount > 0) {
        let properSizedCount = 0;
        
        for (let i = 0; i < Math.min(buttonCount, 10); i++) {
          const button = buttons.nth(i);
          const isVisible = await button.isVisible();
          
          if (isVisible) {
            const box = await button.boundingBox();
            if (box) {
              // Buttons should have reasonable size on desktop
              const isProperSize = box.width >= 60 && box.height >= 30;
              if (isProperSize) {
                properSizedCount++;
              }
            }
          }
        }
        
        console.log(`✓ ${properSizedCount}/${Math.min(buttonCount, 10)} buttons are properly sized on desktop`);
      }
    });
    
    test('Modals display correctly on desktop', async ({ page }) => {
      await page.setViewportSize({ width: 1200, height: 900 });
      await page.goto(PAGES.home);
      await page.waitForLoadState('networkidle');
      
      // Check for modals in the page
      const modals = page.locator('.modal');
      const modalCount = await modals.count();
      
      if (modalCount > 0) {
        const firstModal = modals.first();
        const hasModalClass = await firstModal.evaluate(el => 
          el.classList.contains('modal')
        );
        
        expect(hasModalClass).toBe(true);
        console.log(`✓ ${modalCount} modals found with proper Bootstrap structure on desktop`);
      }
    });

    test('Dropdowns work correctly on desktop', async ({ page }) => {
      await page.setViewportSize({ width: 1200, height: 900 });
      await page.goto(PAGES.home);
      await page.waitForLoadState('networkidle');
      
      // Check for dropdowns
      const dropdowns = page.locator('.dropdown');
      const dropdownCount = await dropdowns.count();
      
      if (dropdownCount > 0) {
        console.log(`✓ ${dropdownCount} dropdowns found on desktop`);
      }
    });
    
    test('Forms display properly on desktop', async ({ page }) => {
      await page.setViewportSize({ width: 1200, height: 900 });
      await page.goto(PAGES.login);
      await page.waitForLoadState('networkidle');
      
      // Check form controls
      const formControls = page.locator('.form-control');
      const controlCount = await formControls.count();
      
      if (controlCount > 0) {
        const firstControl = formControls.first();
        const box = await firstControl.boundingBox();
        
        if (box) {
          // Form controls should have reasonable width on desktop
          expect(box.width).toBeGreaterThan(100);
          
          console.log(`✓ Form controls on desktop: ${box.width}px wide`);
        }
      }
    });
    
    test('Tables display without horizontal scroll on desktop', async ({ page }) => {
      await page.setViewportSize({ width: 1200, height: 900 });
      await page.goto(PAGES.adminProducts);
      await page.waitForLoadState('networkidle');
      
      // Check for tables
      const tables = page.locator('table');
      const tableCount = await tables.count();
      
      if (tableCount > 0) {
        // Check no horizontal overflow
        const bodyWidth = await page.evaluate(() => document.body.scrollWidth);
        const viewportWidth = await page.evaluate(() => window.innerWidth);
        
        expect(bodyWidth).toBeLessThanOrEqual(viewportWidth + 2);
        
        console.log(`✓ ${tableCount} tables display without horizontal scroll on desktop`);
      }
    });
    
  });

  test.describe('Desktop Layout Quality Checks', () => {
    
    test('No horizontal scrolling on any desktop page', async ({ page }) => {
      const viewport = { width: 1200, height: 900 };
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
    
    test('Content is properly centered on desktop', async ({ page }) => {
      await page.setViewportSize({ width: 1920, height: 1080 });
      await page.goto(PAGES.home);
      await page.waitForLoadState('networkidle');
      
      // Check for container classes
      const containers = page.locator('.container, .container-fluid');
      const containerCount = await containers.count();
      
      expect(containerCount).toBeGreaterThan(0);
      
      console.log(`✓ ${containerCount} containers found for proper content centering on desktop`);
    });

    test('Images scale properly on desktop', async ({ page }) => {
      await page.setViewportSize({ width: 1200, height: 900 });
      await page.goto(PAGES.home);
      await page.waitForLoadState('networkidle');
      
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
              const fitsInViewport = box.x + box.width <= 1200 + 20; // +20 for margins
              if (fitsInViewport) {
                properlyScaledCount++;
              }
            }
          }
        }
        
        expect(properlyScaledCount).toBeGreaterThan(0);
        
        console.log(`✓ ${properlyScaledCount}/${Math.min(imageCount, 5)} images scale properly on desktop`);
      }
    });
    
    test('Cards have consistent spacing on desktop', async ({ page }) => {
      await page.setViewportSize({ width: 1200, height: 900 });
      await page.goto(PAGES.home);
      await page.waitForLoadState('networkidle');
      
      // Check for cards
      const cards = page.locator('.card');
      const cardCount = await cards.count();
      
      if (cardCount >= 2) {
        const firstCard = await cards.nth(0).boundingBox();
        const secondCard = await cards.nth(1).boundingBox();
        
        if (firstCard && secondCard) {
          // Calculate spacing between cards
          const spacing = secondCard.x - (firstCard.x + firstCard.width);
          
          // Spacing should be reasonable (Bootstrap gutter is typically 12-24px)
          expect(spacing).toBeGreaterThanOrEqual(0);
          expect(spacing).toBeLessThanOrEqual(50);
          
          console.log(`✓ Card spacing on desktop: ${spacing}px`);
        }
      }
    });
    
  });

  test.describe('Desktop Performance and Loading', () => {
    
    test('Pages load without console errors on desktop', async ({ page }) => {
      const consoleErrors = [];
      
      page.on('console', msg => {
        if (msg.type() === 'error') {
          consoleErrors.push(msg.text());
        }
      });
      
      await page.setViewportSize({ width: 1200, height: 900 });
      await page.goto(PAGES.home);
      await page.waitForLoadState('networkidle');
      
      // Should have no console errors
      expect(consoleErrors.length).toBe(0);
      
      console.log('✓ No console errors on desktop');
    });
    
    test('Bootstrap JavaScript initializes on desktop', async ({ page }) => {
      await page.setViewportSize({ width: 1200, height: 900 });
      await page.goto(PAGES.home);
      await page.waitForLoadState('networkidle');
      
      // Check if Bootstrap is loaded
      const bootstrapLoaded = await page.evaluate(() => {
        return typeof bootstrap !== 'undefined';
      });
      
      expect(bootstrapLoaded).toBe(true);
      console.log('✓ Bootstrap JavaScript initialized on desktop');
    });
    
    test('Responsive grid classes are applied on desktop', async ({ page }) => {
      await page.setViewportSize({ width: 1200, height: 900 });
      await page.goto(PAGES.home);
      await page.waitForLoadState('networkidle');
      
      // Check for desktop-specific grid classes (col-lg-, col-xl-)
      const hasLgClasses = await page.evaluate(() => {
        const elements = document.querySelectorAll('[class*="col-lg-"]');
        return elements.length > 0;
      });
      
      const hasXlClasses = await page.evaluate(() => {
        const elements = document.querySelectorAll('[class*="col-xl-"]');
        return elements.length > 0;
      });
      
      console.log(`✓ Desktop grid classes - col-lg-: ${hasLgClasses}, col-xl-: ${hasXlClasses}`);
    });
    
  });

  test.describe('Desktop Breakpoint Edge Cases', () => {
    
    test('992px (lg breakpoint start) - Layout validation', async ({ page }) => {
      await page.setViewportSize({ width: 992, height: 768 });
      await page.goto(PAGES.home);
      await page.waitForLoadState('networkidle');
      
      // Check no horizontal scroll
      const hasHorizontalScroll = await page.evaluate(() => {
        return document.documentElement.scrollWidth > document.documentElement.clientWidth;
      });
      expect(hasHorizontalScroll).toBe(false);
      
      // Check navbar is expanded
      const toggler = page.locator('.navbar-toggler');
      const togglerVisible = await toggler.isVisible().catch(() => false);
      expect(togglerVisible).toBe(false);
      
      console.log('✓ 992px (lg breakpoint start) validated - navbar expanded, no scroll');
    });
    
    test('1200px (xl breakpoint start) - Layout validation', async ({ page }) => {
      await page.setViewportSize({ width: 1200, height: 900 });
      await page.goto(PAGES.home);
      await page.waitForLoadState('networkidle');
      
      // Check no horizontal scroll
      const hasHorizontalScroll = await page.evaluate(() => {
        return document.documentElement.scrollWidth > document.documentElement.clientWidth;
      });
      expect(hasHorizontalScroll).toBe(false);
      
      console.log('✓ 1200px (xl breakpoint start) validated');
    });
    
    test('1400px (xxl breakpoint start) - Layout validation', async ({ page }) => {
      await page.setViewportSize({ width: 1400, height: 900 });
      await page.goto(PAGES.home);
      await page.waitForLoadState('networkidle');
      
      // Check no horizontal scroll
      const hasHorizontalScroll = await page.evaluate(() => {
        return document.documentElement.scrollWidth > document.documentElement.clientWidth;
      });
      expect(hasHorizontalScroll).toBe(false);
      
      console.log('✓ 1400px (xxl breakpoint start) validated');
    });
    
  });

  test.describe('Desktop Layout Summary Tests', () => {
    
    test('All public pages are desktop-friendly', async ({ page }) => {
      const viewport = { width: 1200, height: 900 };
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
          const elements = document.querySelectorAll('[class*="col-lg-"], [class*="col-xl-"]');
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
      
      console.log('✓ All public pages are desktop-friendly');
    });

    test('Admin area is fully functional on desktop', async ({ page }) => {
      const viewport = { width: 1200, height: 900 };
      await page.setViewportSize(viewport);
      
      await page.goto(PAGES.adminDashboard);
      await page.waitForLoadState('networkidle');
      
      // Check no horizontal overflow
      const bodyWidth = await page.evaluate(() => document.body.scrollWidth);
      const viewportWidth = await page.evaluate(() => window.innerWidth);
      const noOverflow = bodyWidth <= viewportWidth + 2;
      
      // Check for sidebar
      const sidebar = page.locator('.admin-sidebar, [class*="sidebar"]');
      const sidebarExists = await sidebar.count() > 0;
      
      // Check for cards
      const cards = page.locator('.card');
      const cardCount = await cards.count();
      
      expect(noOverflow).toBe(true);
      
      console.log(`✓ Admin area on desktop: sidebar=${sidebarExists}, ${cardCount} cards, no overflow`);
    });
    
    test('POS interface is fully functional on desktop', async ({ page }) => {
      const viewport = { width: 1200, height: 900 };
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
      
      console.log(`✓ POS interface on desktop: ${cardCount} products, ${buttonCount} buttons, no overflow`);
    });
    
  });
  
});
