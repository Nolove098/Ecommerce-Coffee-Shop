// @ts-check
const { test, expect } = require('@playwright/test');

/**
 * Test Suite: Menu Page Mobile Sidebar Toggle
 * 
 * Purpose: Verify that the mobile sidebar toggle button works correctly
 * on the Menu page according to Requirement 6.2
 * 
 * Requirements Tested:
 * - 6.1: Sidebar hidden by default on mobile (< 768px)
 * - 6.2: Toggle button visible on mobile and toggles sidebar
 * - 6.3: Sidebar auto-closes when category is selected
 */

test.describe('Menu Page Mobile Sidebar Toggle', () => {
  const MENU_URL = '/Menu';
  const MOBILE_VIEWPORT = { width: 375, height: 667 }; // iPhone SE
  const DESKTOP_VIEWPORT = { width: 1280, height: 720 };

  test('Requirement 6.2: Toggle button is visible only on mobile', async ({ page }) => {
    // Test on mobile viewport
    await page.setViewportSize(MOBILE_VIEWPORT);
    await page.goto(MENU_URL);
    await page.waitForLoadState('networkidle');

    const toggleButton = page.locator('#sidebarToggle');
    await expect(toggleButton).toBeVisible();
    console.log('✓ Toggle button visible on mobile (375px)');

    // Test on desktop viewport
    await page.setViewportSize(DESKTOP_VIEWPORT);
    await page.goto(MENU_URL);
    await page.waitForLoadState('networkidle');

    await expect(toggleButton).toBeHidden();
    console.log('✓ Toggle button hidden on desktop (1280px)');
  });

  test('Requirement 6.1 & 6.2: Sidebar hidden by default on mobile and toggles correctly', async ({ page }) => {
    await page.setViewportSize(MOBILE_VIEWPORT);
    await page.goto(MENU_URL);
    await page.waitForLoadState('networkidle');

    const sidebar = page.locator('.category-sidebar');
    const overlay = page.locator('.sidebar-overlay');
    const toggleButton = page.locator('#sidebarToggle');

    // Sidebar should be hidden by default (no .show class)
    const sidebarHasShowClass = await sidebar.evaluate(el => el.classList.contains('show'));
    expect(sidebarHasShowClass).toBe(false);
    console.log('✓ Sidebar hidden by default on mobile');

    // Overlay should be hidden by default
    const overlayHasShowClass = await overlay.evaluate(el => el.classList.contains('show'));
    expect(overlayHasShowClass).toBe(false);
    console.log('✓ Overlay hidden by default');

    // Click toggle button to open sidebar
    await toggleButton.click();
    await page.waitForTimeout(400); // Wait for CSS transition

    // Sidebar should now have .show class
    const sidebarShownAfterClick = await sidebar.evaluate(el => el.classList.contains('show'));
    expect(sidebarShownAfterClick).toBe(true);
    console.log('✓ Sidebar opens when toggle button clicked');

    // Overlay should now be visible
    const overlayShownAfterClick = await overlay.evaluate(el => el.classList.contains('show'));
    expect(overlayShownAfterClick).toBe(true);
    console.log('✓ Overlay appears when sidebar opens');

    // Click toggle button again to close sidebar
    await toggleButton.click();
    await page.waitForTimeout(400);

    const sidebarHiddenAfterSecondClick = await sidebar.evaluate(el => el.classList.contains('show'));
    expect(sidebarHiddenAfterSecondClick).toBe(false);
    console.log('✓ Sidebar closes when toggle button clicked again');
  });

  test('Requirement 6.2: Clicking overlay closes sidebar', async ({ page }) => {
    await page.setViewportSize(MOBILE_VIEWPORT);
    await page.goto(MENU_URL);
    await page.waitForLoadState('networkidle');

    const sidebar = page.locator('.category-sidebar');
    const overlay = page.locator('.sidebar-overlay');
    const toggleButton = page.locator('#sidebarToggle');

    // Open sidebar
    await toggleButton.click();
    await page.waitForTimeout(400);

    // Verify sidebar is open
    let sidebarShown = await sidebar.evaluate(el => el.classList.contains('show'));
    expect(sidebarShown).toBe(true);

    // Click overlay to close
    await overlay.click();
    await page.waitForTimeout(400);

    // Verify sidebar is closed
    sidebarShown = await sidebar.evaluate(el => el.classList.contains('show'));
    expect(sidebarShown).toBe(false);
    console.log('✓ Clicking overlay closes sidebar');
  });

  test('Requirement 6.3: Sidebar auto-closes when category is selected on mobile', async ({ page }) => {
    await page.setViewportSize(MOBILE_VIEWPORT);
    await page.goto(MENU_URL);
    await page.waitForLoadState('networkidle');

    const sidebar = page.locator('.category-sidebar');
    const toggleButton = page.locator('#sidebarToggle');

    // Open sidebar
    await toggleButton.click();
    await page.waitForTimeout(400);

    // Verify sidebar is open
    let sidebarShown = await sidebar.evaluate(el => el.classList.contains('show'));
    expect(sidebarShown).toBe(true);
    console.log('✓ Sidebar opened');

    // Click a category link (first category in the list)
    const categoryLinks = page.locator('.category-sidebar a');
    const firstCategory = categoryLinks.first();
    
    // Wait for navigation to complete
    await Promise.all([
      page.waitForNavigation({ waitUntil: 'networkidle' }),
      firstCategory.click()
    ]);

    // After navigation, sidebar should be closed (no .show class)
    sidebarShown = await sidebar.evaluate(el => el.classList.contains('show'));
    expect(sidebarShown).toBe(false);
    console.log('✓ Sidebar auto-closes after category selection on mobile');
  });

  test('Toggle button has correct styling and icon', async ({ page }) => {
    await page.setViewportSize(MOBILE_VIEWPORT);
    await page.goto(MENU_URL);
    await page.waitForLoadState('networkidle');

    const toggleButton = page.locator('#sidebarToggle');

    // Check button has correct classes
    const buttonClasses = await toggleButton.getAttribute('class');
    expect(buttonClasses).toContain('btn');
    expect(buttonClasses).toContain('btn-outline-primary');
    expect(buttonClasses).toContain('d-md-none');
    console.log('✓ Toggle button has correct Bootstrap classes');

    // Check button contains SVG icon
    const svgIcon = toggleButton.locator('svg');
    await expect(svgIcon).toBeVisible();
    console.log('✓ Toggle button contains hamburger icon');

    // Check button contains text
    const buttonText = await toggleButton.textContent();
    expect(buttonText).toContain('Danh mục');
    console.log('✓ Toggle button contains "Danh mục" text');
  });
});
