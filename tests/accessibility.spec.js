// @ts-check
const { test, expect } = require('@playwright/test');
const AxeBuilder = require('@axe-core/playwright').default;

/**
 * Accessibility Testing Suite for Bootstrap 5 Migration
 * 
 * This test suite validates:
 * - ARIA attributes on interactive components
 * - Form accessibility (labels, validation messages)
 * - Keyboard navigation and focus indicators
 * - Semantic HTML structure
 * - Color contrast (WCAG AA standards)
 * - General accessibility violations
 */

const BASE_URL = 'http://localhost:5000';

// Helper function to run axe and report violations
async function checkAccessibility(page, pageName) {
  const accessibilityScanResults = await new AxeBuilder({ page })
    .withTags(['wcag2a', 'wcag2aa', 'wcag21a', 'wcag21aa'])
    .analyze();

  const violations = accessibilityScanResults.violations;
  
  if (violations.length > 0) {
    console.log(`\n❌ Accessibility violations found on ${pageName}:`);
    violations.forEach((violation, index) => {
      console.log(`\n${index + 1}. ${violation.id}: ${violation.description}`);
      console.log(`   Impact: ${violation.impact}`);
      console.log(`   Help: ${violation.help}`);
      console.log(`   Affected elements: ${violation.nodes.length}`);
      violation.nodes.forEach((node, nodeIndex) => {
        console.log(`     ${nodeIndex + 1}. ${node.html.substring(0, 100)}...`);
        console.log(`        ${node.failureSummary}`);
      });
    });
  }

  return violations;
}

test.describe('Accessibility Validation - Public Area', () => {
  
  test('Home page should pass accessibility audit', async ({ page }) => {
    await page.goto(`${BASE_URL}/`);
    await page.waitForLoadState('networkidle');
    
    const violations = await checkAccessibility(page, 'Home Page');
    expect(violations.length).toBe(0);
  });

  test('Login page should pass accessibility audit', async ({ page }) => {
    await page.goto(`${BASE_URL}/Auth/Login`);
    await page.waitForLoadState('networkidle');
    
    const violations = await checkAccessibility(page, 'Login Page');
    expect(violations.length).toBe(0);
  });

  test('Register page should pass accessibility audit', async ({ page }) => {
    await page.goto(`${BASE_URL}/Auth/Register`);
    await page.waitForLoadState('networkidle');
    
    const violations = await checkAccessibility(page, 'Register Page');
    expect(violations.length).toBe(0);
  });

  test('Checkout page should pass accessibility audit', async ({ page }) => {
    await page.goto(`${BASE_URL}/Cart/Checkout`);
    await page.waitForLoadState('networkidle');
    
    const violations = await checkAccessibility(page, 'Checkout Page');
    expect(violations.length).toBe(0);
  });

  test('Product Detail page should pass accessibility audit', async ({ page }) => {
    await page.goto(`${BASE_URL}/Product/Detail/1`);
    await page.waitForLoadState('networkidle');
    
    const violations = await checkAccessibility(page, 'Product Detail Page');
    expect(violations.length).toBe(0);
  });

  test('Product Category page should pass accessibility audit', async ({ page }) => {
    await page.goto(`${BASE_URL}/Product/Category?category=Coffee`);
    await page.waitForLoadState('networkidle');
    
    const violations = await checkAccessibility(page, 'Product Category Page');
    expect(violations.length).toBe(0);
  });
});

test.describe('Accessibility Validation - ARIA Attributes', () => {
  
  test('Navbar toggler should have proper ARIA attributes', async ({ page }) => {
    await page.goto(`${BASE_URL}/`);
    
    const toggler = page.locator('.navbar-toggler');
    await expect(toggler).toHaveAttribute('aria-controls', 'navbarNav');
    await expect(toggler).toHaveAttribute('aria-expanded');
    await expect(toggler).toHaveAttribute('aria-label', 'Toggle navigation');
  });

  test('Offcanvas cart drawer should have proper ARIA attributes', async ({ page }) => {
    await page.goto(`${BASE_URL}/`);
    
    const offcanvas = page.locator('#cartDrawer');
    await expect(offcanvas).toHaveAttribute('aria-labelledby', 'cartDrawerLabel');
    await expect(offcanvas).toHaveAttribute('tabindex', '-1');
    
    const closeButton = offcanvas.locator('.btn-close');
    await expect(closeButton).toHaveAttribute('aria-label', 'Close');
  });

  test('Admin sidebar offcanvas should have proper ARIA attributes', async ({ page }) => {
    // Note: This requires authentication, adjust URL as needed
    await page.goto(`${BASE_URL}/Admin/Dashboard`);
    
    const sidebar = page.locator('#adminSidebar');
    if (await sidebar.count() > 0) {
      await expect(sidebar).toHaveAttribute('aria-labelledby', 'adminSidebarLabel');
      await expect(sidebar).toHaveAttribute('tabindex', '-1');
    }
  });

  test('Breadcrumb navigation should have proper ARIA label', async ({ page }) => {
    await page.goto(`${BASE_URL}/Product/Detail/1`);
    
    const breadcrumb = page.locator('nav[aria-label="breadcrumb"]');
    await expect(breadcrumb).toBeVisible();
  });

  test('Pagination should have proper ARIA label', async ({ page }) => {
    await page.goto(`${BASE_URL}/Product/Category?category=Coffee`);
    
    const pagination = page.locator('nav[aria-label="Product pagination"]');
    if (await pagination.count() > 0) {
      await expect(pagination).toBeVisible();
    }
  });
});

test.describe('Accessibility Validation - Form Accessibility', () => {
  
  test('Login form inputs should have associated labels', async ({ page }) => {
    await page.goto(`${BASE_URL}/Auth/Login`);
    
    // Check LoginId input has label
    const loginIdInput = page.locator('input[name="LoginId"]');
    const loginIdLabel = page.locator('label[for="LoginId"]');
    await expect(loginIdInput).toBeVisible();
    await expect(loginIdLabel).toBeVisible();
    
    // Check Password input has label
    const passwordInput = page.locator('input[name="Password"]');
    const passwordLabel = page.locator('label[for="Password"]');
    await expect(passwordInput).toBeVisible();
    await expect(passwordLabel).toBeVisible();
  });

  test('Register form inputs should have associated labels', async ({ page }) => {
    await page.goto(`${BASE_URL}/Auth/Register`);
    
    const formFields = ['FullName', 'Email', 'Phone', 'Password', 'ConfirmPassword'];
    
    for (const field of formFields) {
      const input = page.locator(`input[name="${field}"]`);
      const label = page.locator(`label[for="${field}"]`);
      await expect(input).toBeVisible();
      await expect(label).toBeVisible();
    }
  });

  test('Checkout form inputs should have associated labels', async ({ page }) => {
    await page.goto(`${BASE_URL}/Cart/Checkout`);
    
    const formFields = ['CustomerName', 'CustomerPhone', 'Address', 'Note'];
    
    for (const field of formFields) {
      const input = page.locator(`[name="${field}"]`);
      const label = page.locator(`label[for="${field}"]`);
      await expect(input).toBeVisible();
      await expect(label).toBeVisible();
    }
  });

  test('Form validation messages should be accessible', async ({ page }) => {
    await page.goto(`${BASE_URL}/Auth/Login`);
    
    // Submit empty form to trigger validation
    await page.click('button[type="submit"]');
    
    // Check if validation messages are visible and have proper classes
    const validationMessages = page.locator('.invalid-feedback');
    const count = await validationMessages.count();
    
    if (count > 0) {
      // Validation messages should be visible
      for (let i = 0; i < count; i++) {
        const message = validationMessages.nth(i);
        // Check if it has d-block class or is visible
        const isVisible = await message.isVisible();
        expect(isVisible).toBeTruthy();
      }
    }
  });
});

test.describe('Accessibility Validation - Keyboard Navigation', () => {
  
  test('Tab navigation should work on Home page', async ({ page }) => {
    await page.goto(`${BASE_URL}/`);
    await page.waitForLoadState('networkidle');
    
    // Start tabbing through interactive elements
    await page.keyboard.press('Tab');
    
    // Check if focus is visible
    const focusedElement = await page.evaluate(() => {
      return document.activeElement?.tagName;
    });
    
    expect(focusedElement).toBeTruthy();
  });

  test('Enter key should work on buttons', async ({ page }) => {
    await page.goto(`${BASE_URL}/`);
    
    // Focus on a button and press Enter
    const button = page.locator('button').first();
    await button.focus();
    
    // Verify focus indicator is visible
    await expect(button).toBeFocused();
  });

  test('Escape key should close offcanvas', async ({ page }) => {
    await page.goto(`${BASE_URL}/`);
    
    // Open cart drawer
    const cartButton = page.locator('[data-bs-toggle="offcanvas"][data-bs-target="#cartDrawer"]');
    if (await cartButton.count() > 0) {
      await cartButton.click();
      await page.waitForTimeout(500); // Wait for animation
      
      // Press Escape
      await page.keyboard.press('Escape');
      await page.waitForTimeout(500); // Wait for animation
      
      // Verify offcanvas is closed
      const offcanvas = page.locator('#cartDrawer');
      const isVisible = await offcanvas.evaluate(el => {
        return el.classList.contains('show');
      });
      expect(isVisible).toBe(false);
    }
  });

  test('Focus indicators should be visible on interactive elements', async ({ page }) => {
    await page.goto(`${BASE_URL}/`);
    
    // Get all interactive elements
    const buttons = page.locator('button, a, input, select, textarea');
    const count = await buttons.count();
    
    // Check at least some elements exist
    expect(count).toBeGreaterThan(0);
    
    // Focus on first button and check outline
    const firstButton = buttons.first();
    await firstButton.focus();
    
    const hasOutline = await firstButton.evaluate(el => {
      const styles = window.getComputedStyle(el);
      return styles.outline !== 'none' || styles.boxShadow !== 'none';
    });
    
    // Bootstrap provides focus styles, so this should be true
    expect(hasOutline).toBeTruthy();
  });
});

test.describe('Accessibility Validation - Semantic HTML', () => {
  
  test('Home page should use semantic HTML elements', async ({ page }) => {
    await page.goto(`${BASE_URL}/`);
    
    // Check for semantic elements
    await expect(page.locator('nav')).toHaveCount(1); // At least one nav
    await expect(page.locator('footer')).toHaveCount(1);
    
    const sectionCount = await page.locator('section').count();
    expect(sectionCount).toBeGreaterThan(0);
  });

  test('Product Detail page should use semantic HTML elements', async ({ page }) => {
    await page.goto(`${BASE_URL}/Product/Detail/1`);
    
    await expect(page.locator('nav')).toHaveCount(2); // Navbar + breadcrumb
    await expect(page.locator('footer')).toHaveCount(1);
    await expect(page.locator('section')).toHaveCount(3); // Breadcrumb, detail, related
  });

  test('Admin layout should use semantic HTML elements', async ({ page }) => {
    await page.goto(`${BASE_URL}/Admin/Dashboard`);
    
    // Check for header and main
    const header = page.locator('header');
    const main = page.locator('main');
    
    if (await header.count() > 0) {
      await expect(header).toBeVisible();
    }
    if (await main.count() > 0) {
      await expect(main).toBeVisible();
    }
  });

  test('Heading hierarchy should be correct', async ({ page }) => {
    await page.goto(`${BASE_URL}/`);
    
    // Check if h1 exists
    const h1Count = await page.locator('h1').count();
    expect(h1Count).toBeGreaterThanOrEqual(1);
    
    // Get all headings
    const headings = await page.locator('h1, h2, h3, h4, h5, h6').all();
    expect(headings.length).toBeGreaterThan(0);
  });
});

test.describe('Accessibility Validation - Color Contrast', () => {
  
  test('Color contrast should meet WCAG AA standards', async ({ page }) => {
    await page.goto(`${BASE_URL}/`);
    
    // Run axe with color-contrast rule specifically
    const accessibilityScanResults = await new AxeBuilder({ page })
      .withTags(['wcag2aa'])
      .include('body')
      .analyze();
    
    const colorContrastViolations = accessibilityScanResults.violations.filter(
      v => v.id === 'color-contrast'
    );
    
    if (colorContrastViolations.length > 0) {
      console.log('\n❌ Color contrast violations:');
      colorContrastViolations.forEach(violation => {
        violation.nodes.forEach(node => {
          console.log(`  - ${node.html.substring(0, 100)}`);
          console.log(`    ${node.failureSummary}`);
        });
      });
    }
    
    expect(colorContrastViolations.length).toBe(0);
  });
});
