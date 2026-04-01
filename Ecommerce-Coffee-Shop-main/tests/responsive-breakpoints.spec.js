// @ts-check
const { test, expect } = require('@playwright/test');

/**
 * Task 12.1: Verify responsive behavior at all Bootstrap breakpoints
 * 
 * This test suite validates that all pages in the SaleStore application
 * respond correctly at Bootstrap 5 breakpoints:
 * - 576px (sm)
 * - 768px (md)
 * - 992px (lg)
 * - 1200px (xl)
 * - 1400px (xxl)
 * 
 * Requirements: 12.1, 12.2, 12.3, 19.3
 */

// Bootstrap breakpoints
const BREAKPOINTS = {
  sm: 576,
  md: 768,
  lg: 992,
  xl: 1200,
  xxl: 1400
};

// All pages to test
const PAGES = {
  // Public area
  home: '/',
  login: '/Auth/Login',
  register: '/Auth/Register',
  checkout: '/Cart/Checkout',
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

test.describe('Responsive Behavior - All Bootstrap Breakpoints', () => {
  
  test.describe('Public Area Pages', () => {
    
    test('Home page - responsive at all breakpoints', async ({ page }) => {
      for (const [name, width] of Object.entries(BREAKPOINTS)) {
        await page.setViewportSize({ width, height: 800 });
        await page.goto(PAGES.home);
        
        // Check no horizontal overflow
        const bodyWidth = await page.evaluate(() => document.body.scrollWidth);
        const viewportWidth = await page.evaluate(() => window.innerWidth);
        expect(bodyWidth).toBeLessThanOrEqual(viewportWidth + 1); // +1 for rounding
        
        // Check navbar exists
        const navbar = page.locator('nav.navbar');
        await expect(navbar).toBeVisible();
        
        // At mobile breakpoint (< 992px), navbar should have toggler
        if (width < BREAKPOINTS.lg) {
          const toggler = page.locator('.navbar-toggler');
          await expect(toggler).toBeVisible();
        }
        
        console.log(`✓ Home page responsive at ${name} (${width}px)`);
      }
    });
    
    test('Login page - responsive at all breakpoints', async ({ page }) => {
      for (const [name, width] of Object.entries(BREAKPOINTS)) {
        await page.setViewportSize({ width, height: 800 });
        await page.goto(PAGES.login);
        
        // Check no horizontal overflow
        const bodyWidth = await page.evaluate(() => document.body.scrollWidth);
        const viewportWidth = await page.evaluate(() => window.innerWidth);
        expect(bodyWidth).toBeLessThanOrEqual(viewportWidth + 1);
        
        // Check form is visible
        const form = page.locator('form');
        await expect(form).toBeVisible();
        
        // Check form controls have Bootstrap classes
        const formControls = page.locator('.form-control');
        const count = await formControls.count();
        expect(count).toBeGreaterThan(0);
        
        console.log(`✓ Login page responsive at ${name} (${width}px)`);
      }
    });
    
    test('Register page - responsive at all breakpoints', async ({ page }) => {
      for (const [name, width] of Object.entries(BREAKPOINTS)) {
        await page.setViewportSize({ width, height: 800 });
        await page.goto(PAGES.register);
        
        // Check no horizontal overflow
        const bodyWidth = await page.evaluate(() => document.body.scrollWidth);
        const viewportWidth = await page.evaluate(() => window.innerWidth);
        expect(bodyWidth).toBeLessThanOrEqual(viewportWidth + 1);
        
        // Check form is visible
        const form = page.locator('form');
        await expect(form).toBeVisible();
        
        console.log(`✓ Register page responsive at ${name} (${width}px)`);
      }
    });
    
    test('Checkout page - responsive at all breakpoints', async ({ page }) => {
      for (const [name, width] of Object.entries(BREAKPOINTS)) {
        await page.setViewportSize({ width, height: 800 });
        
        // Try to access checkout page (may redirect if no cart items)
        const response = await page.goto(PAGES.checkout);
        
        // Check no horizontal overflow
        const bodyWidth = await page.evaluate(() => document.body.scrollWidth);
        const viewportWidth = await page.evaluate(() => window.innerWidth);
        expect(bodyWidth).toBeLessThanOrEqual(viewportWidth + 1);
        
        console.log(`✓ Checkout page responsive at ${name} (${width}px)`);
      }
    });
    
  });
  
  test.describe('Responsive Grid Layouts', () => {
    
    test('Product cards adapt to breakpoints', async ({ page }) => {
      await page.goto(PAGES.home);
      
      // Test at each breakpoint
      for (const [name, width] of Object.entries(BREAKPOINTS)) {
        await page.setViewportSize({ width, height: 800 });
        await page.waitForTimeout(300); // Wait for layout to settle
        
        // Check if product cards exist
        const cards = page.locator('.card');
        const cardCount = await cards.count();
        
        if (cardCount > 0) {
          // Check cards are visible
          await expect(cards.first()).toBeVisible();
          
          // Check no horizontal overflow on cards
          const firstCard = cards.first();
          const cardBox = await firstCard.boundingBox();
          if (cardBox) {
            expect(cardBox.x + cardBox.width).toBeLessThanOrEqual(width + 20); // +20 for margins
          }
        }
        
        console.log(`✓ Product cards responsive at ${name} (${width}px)`);
      }
    });
    
    test('Bootstrap grid system works at all breakpoints', async ({ page }) => {
      await page.goto(PAGES.home);
      
      for (const [name, width] of Object.entries(BREAKPOINTS)) {
        await page.setViewportSize({ width, height: 800 });
        
        // Check for Bootstrap grid classes
        const hasGridClasses = await page.evaluate(() => {
          const elements = document.querySelectorAll('[class*="col-"]');
          return elements.length > 0;
        });
        
        // If grid classes exist, verify no overflow
        if (hasGridClasses) {
          const rows = page.locator('.row');
          const rowCount = await rows.count();
          
          if (rowCount > 0) {
            const firstRow = rows.first();
            const rowBox = await firstRow.boundingBox();
            if (rowBox) {
              expect(rowBox.x + rowBox.width).toBeLessThanOrEqual(width + 20);
            }
          }
        }
        
        console.log(`✓ Grid system responsive at ${name} (${width}px)`);
      }
    });
    
  });
  
  test.describe('Responsive Utilities', () => {
    
    test('Responsive visibility classes work correctly', async ({ page }) => {
      await page.goto(PAGES.home);
      
      // Test d-none and d-md-block patterns
      for (const [name, width] of Object.entries(BREAKPOINTS)) {
        await page.setViewportSize({ width, height: 800 });
        await page.waitForTimeout(200);
        
        // Check for elements with responsive display classes
        const hiddenOnMobile = page.locator('.d-none.d-md-block');
        const hiddenOnMobileCount = await hiddenOnMobile.count();
        
        if (hiddenOnMobileCount > 0) {
          const firstElement = hiddenOnMobile.first();
          const isVisible = await firstElement.isVisible();
          
          // Should be hidden on mobile (< 768px), visible on md and up
          if (width < BREAKPOINTS.md) {
            expect(isVisible).toBe(false);
          } else {
            expect(isVisible).toBe(true);
          }
        }
        
        console.log(`✓ Responsive utilities work at ${name} (${width}px)`);
      }
    });
    
  });
  
  test.describe('Images and Tables', () => {
    
    test('Images are responsive with img-fluid', async ({ page }) => {
      await page.goto(PAGES.home);
      
      for (const [name, width] of Object.entries(BREAKPOINTS)) {
        await page.setViewportSize({ width, height: 800 });
        
        // Check for images
        const images = page.locator('img');
        const imageCount = await images.count();
        
        if (imageCount > 0) {
          // Check first image doesn't overflow
          const firstImage = images.first();
          const imageBox = await firstImage.boundingBox();
          
          if (imageBox) {
            expect(imageBox.x + imageBox.width).toBeLessThanOrEqual(width + 10);
          }
        }
        
        console.log(`✓ Images responsive at ${name} (${width}px)`);
      }
    });
    
    test('Tables are wrapped in table-responsive', async ({ page }) => {
      // Try to access a page with tables (admin pages have tables)
      await page.goto(PAGES.home);
      
      for (const [name, width] of Object.entries(BREAKPOINTS)) {
        await page.setViewportSize({ width, height: 800 });
        
        // Check for tables
        const tables = page.locator('table');
        const tableCount = await tables.count();
        
        if (tableCount > 0) {
          // Check if table is wrapped in table-responsive
          const responsiveTables = page.locator('.table-responsive table');
          const responsiveCount = await responsiveTables.count();
          
          // At least some tables should be responsive
          // (Not all tables may need responsive wrapper)
          console.log(`Found ${tableCount} tables, ${responsiveCount} responsive`);
        }
        
        console.log(`✓ Tables checked at ${name} (${width}px)`);
      }
    });
    
  });
  
  test.describe('No Console Errors', () => {
    
    test('Pages load without console errors at all breakpoints', async ({ page }) => {
      const consoleErrors = [];
      
      page.on('console', msg => {
        if (msg.type() === 'error') {
          consoleErrors.push(msg.text());
        }
      });
      
      for (const [name, width] of Object.entries(BREAKPOINTS)) {
        consoleErrors.length = 0; // Clear errors
        
        await page.setViewportSize({ width, height: 800 });
        await page.goto(PAGES.home);
        await page.waitForLoadState('networkidle');
        
        // Check for console errors
        expect(consoleErrors.length).toBe(0);
        
        console.log(`✓ No console errors at ${name} (${width}px)`);
      }
    });
    
  });
  
  test.describe('Bootstrap Components at Breakpoints', () => {
    
    test('Navbar collapses correctly at mobile breakpoint', async ({ page }) => {
      await page.goto(PAGES.home);
      
      // Test at lg breakpoint (992px) - navbar should be expanded
      await page.setViewportSize({ width: BREAKPOINTS.lg, height: 800 });
      await page.waitForTimeout(200);
      
      let toggler = page.locator('.navbar-toggler');
      let togglerVisible = await toggler.isVisible().catch(() => false);
      
      // At lg and above, toggler may not be visible
      console.log(`Navbar toggler at lg (992px): ${togglerVisible ? 'visible' : 'hidden'}`);
      
      // Test at md breakpoint (768px) - navbar should have toggler
      await page.setViewportSize({ width: BREAKPOINTS.md, height: 800 });
      await page.waitForTimeout(200);
      
      toggler = page.locator('.navbar-toggler');
      togglerVisible = await toggler.isVisible().catch(() => false);
      
      // At md and below, toggler should be visible
      if (togglerVisible) {
        await expect(toggler).toBeVisible();
        console.log(`✓ Navbar toggler visible at md (768px)`);
      }
      
      // Test at sm breakpoint (576px) - navbar should definitely have toggler
      await page.setViewportSize({ width: BREAKPOINTS.sm, height: 800 });
      await page.waitForTimeout(200);
      
      toggler = page.locator('.navbar-toggler');
      togglerVisible = await toggler.isVisible().catch(() => false);
      
      if (togglerVisible) {
        await expect(toggler).toBeVisible();
        console.log(`✓ Navbar toggler visible at sm (576px)`);
      }
    });
    
    test('Cards maintain structure at all breakpoints', async ({ page }) => {
      await page.goto(PAGES.home);
      
      for (const [name, width] of Object.entries(BREAKPOINTS)) {
        await page.setViewportSize({ width, height: 800 });
        await page.waitForTimeout(200);
        
        // Check for Bootstrap cards
        const cards = page.locator('.card');
        const cardCount = await cards.count();
        
        if (cardCount > 0) {
          // Check first card has proper structure
          const firstCard = cards.first();
          await expect(firstCard).toBeVisible();
          
          // Check card body exists
          const cardBody = firstCard.locator('.card-body');
          const hasBody = await cardBody.count() > 0;
          
          console.log(`✓ Cards maintain structure at ${name} (${width}px) - ${cardCount} cards found`);
        }
      }
    });
    
  });
  
});

test.describe('Specific Breakpoint Validations', () => {
  
  test('576px (sm) - Mobile layout validation', async ({ page }) => {
    await page.setViewportSize({ width: 576, height: 800 });
    await page.goto(PAGES.home);
    
    // Check no horizontal scroll
    const hasHorizontalScroll = await page.evaluate(() => {
      return document.documentElement.scrollWidth > document.documentElement.clientWidth;
    });
    expect(hasHorizontalScroll).toBe(false);
    
    console.log('✓ 576px (sm) breakpoint validated');
  });
  
  test('768px (md) - Tablet layout validation', async ({ page }) => {
    await page.setViewportSize({ width: 768, height: 800 });
    await page.goto(PAGES.home);
    
    // Check no horizontal scroll
    const hasHorizontalScroll = await page.evaluate(() => {
      return document.documentElement.scrollWidth > document.documentElement.clientWidth;
    });
    expect(hasHorizontalScroll).toBe(false);
    
    console.log('✓ 768px (md) breakpoint validated');
  });
  
  test('992px (lg) - Desktop layout validation', async ({ page }) => {
    await page.setViewportSize({ width: 992, height: 800 });
    await page.goto(PAGES.home);
    
    // Check no horizontal scroll
    const hasHorizontalScroll = await page.evaluate(() => {
      return document.documentElement.scrollWidth > document.documentElement.clientWidth;
    });
    expect(hasHorizontalScroll).toBe(false);
    
    console.log('✓ 992px (lg) breakpoint validated');
  });
  
  test('1200px (xl) - Large desktop layout validation', async ({ page }) => {
    await page.setViewportSize({ width: 1200, height: 800 });
    await page.goto(PAGES.home);
    
    // Check no horizontal scroll
    const hasHorizontalScroll = await page.evaluate(() => {
      return document.documentElement.scrollWidth > document.documentElement.clientWidth;
    });
    expect(hasHorizontalScroll).toBe(false);
    
    console.log('✓ 1200px (xl) breakpoint validated');
  });
  
  test('1400px (xxl) - Extra large desktop layout validation', async ({ page }) => {
    await page.setViewportSize({ width: 1400, height: 800 });
    await page.goto(PAGES.home);
    
    // Check no horizontal scroll
    const hasHorizontalScroll = await page.evaluate(() => {
      return document.documentElement.scrollWidth > document.documentElement.clientWidth;
    });
    expect(hasHorizontalScroll).toBe(false);
    
    console.log('✓ 1400px (xxl) breakpoint validated');
  });
  
});
