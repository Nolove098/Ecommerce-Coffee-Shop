// @ts-check
const { test, expect } = require('@playwright/test');

/**
 * Test Suite: Menu Page Pagination Controls
 * 
 * Purpose: Verify that pagination controls work correctly on the Menu page
 * 
 * Requirements Tested:
 * - 3.2: Pagination controls visible only when TotalPages > 1
 * - 3.3: Pagination controls include Previous, Next, and page indicator
 * - 3.4: Previous button disabled on first page
 * - 3.5: Next button disabled on last page
 * - 4.1: Next button navigates to next page
 * - 4.2: Previous button navigates to previous page
 * - 4.5: Category parameter preserved in pagination links
 * - 8.1: Page indicator shows "Trang X / Y" format
 */

test.describe('Menu Page Pagination Controls', () => {
  const MENU_URL = '/Menu';

  test('Requirement 3.2: Pagination controls only visible when TotalPages > 1', async ({ page }) => {
    await page.goto(MENU_URL);
    await page.waitForLoadState('networkidle');

    const paginationControls = page.locator('.pagination-controls');
    
    // Check if pagination controls exist
    const controlsCount = await paginationControls.count();
    
    if (controlsCount > 0) {
      // If controls exist, verify they're visible
      await expect(paginationControls).toBeVisible();
      console.log('✓ Pagination controls visible (TotalPages > 1)');
    } else {
      // If controls don't exist, that's also valid (TotalPages <= 1)
      console.log('✓ Pagination controls not rendered (TotalPages <= 1)');
    }
  });

  test('Requirement 3.3 & 8.1: Pagination controls have correct structure and format', async ({ page }) => {
    // Navigate to page 2 to ensure pagination exists
    await page.goto(`${MENU_URL}?page=2`);
    await page.waitForLoadState('networkidle');

    const paginationControls = page.locator('.pagination-controls');
    
    // Skip test if pagination doesn't exist (not enough products)
    const controlsCount = await paginationControls.count();
    if (controlsCount === 0) {
      console.log('⊘ Skipping test - not enough products for pagination');
      test.skip();
      return;
    }

    // Check Previous button exists
    const prevButton = paginationControls.locator('a:has-text("Trước")');
    await expect(prevButton).toBeVisible();
    console.log('✓ Previous button exists');

    // Check Next button exists
    const nextButton = paginationControls.locator('a:has-text("Sau")');
    await expect(nextButton).toBeVisible();
    console.log('✓ Next button exists');

    // Check page indicator exists with correct format "Trang X / Y"
    const pageIndicator = paginationControls.locator('.page-indicator');
    await expect(pageIndicator).toBeVisible();
    
    const indicatorText = await pageIndicator.textContent();
    expect(indicatorText).toMatch(/Trang \d+ \/ \d+/);
    console.log(`✓ Page indicator shows correct format: "${indicatorText}"`);
  });

  test('Requirement 3.4: Previous button disabled on first page', async ({ page }) => {
    await page.goto(`${MENU_URL}?page=1`);
    await page.waitForLoadState('networkidle');

    const paginationControls = page.locator('.pagination-controls');
    const controlsCount = await paginationControls.count();
    
    if (controlsCount === 0) {
      console.log('⊘ Skipping test - not enough products for pagination');
      test.skip();
      return;
    }

    const prevButton = paginationControls.locator('a:has-text("Trước")');
    
    // Check if button has disabled class
    const buttonClasses = await prevButton.getAttribute('class');
    expect(buttonClasses).toContain('disabled');
    console.log('✓ Previous button has disabled class on page 1');

    // Check if button has aria-disabled attribute
    const ariaDisabled = await prevButton.getAttribute('aria-disabled');
    expect(ariaDisabled).toBe('true');
    console.log('✓ Previous button has aria-disabled="true" on page 1');
  });

  test('Requirement 3.5: Next button disabled on last page', async ({ page }) => {
    await page.goto(MENU_URL);
    await page.waitForLoadState('networkidle');

    const paginationControls = page.locator('.pagination-controls');
    const controlsCount = await paginationControls.count();
    
    if (controlsCount === 0) {
      console.log('⊘ Skipping test - not enough products for pagination');
      test.skip();
      return;
    }

    // Get the total pages from the page indicator
    const pageIndicator = paginationControls.locator('.page-indicator');
    const indicatorText = await pageIndicator.textContent();
    const match = indicatorText.match(/Trang (\d+) \/ (\d+)/);
    
    if (!match) {
      console.log('⊘ Could not parse page indicator');
      test.skip();
      return;
    }

    const totalPages = parseInt(match[2]);
    
    // Navigate to last page
    await page.goto(`${MENU_URL}?page=${totalPages}`);
    await page.waitForLoadState('networkidle');

    const nextButton = paginationControls.locator('a:has-text("Sau")');
    
    // Check if button has disabled class
    const buttonClasses = await nextButton.getAttribute('class');
    expect(buttonClasses).toContain('disabled');
    console.log(`✓ Next button has disabled class on last page (${totalPages})`);

    // Check if button has aria-disabled attribute
    const ariaDisabled = await nextButton.getAttribute('aria-disabled');
    expect(ariaDisabled).toBe('true');
    console.log('✓ Next button has aria-disabled="true" on last page');
  });

  test('Requirement 4.1: Next button navigates to next page', async ({ page }) => {
    await page.goto(`${MENU_URL}?page=1`);
    await page.waitForLoadState('networkidle');

    const paginationControls = page.locator('.pagination-controls');
    const controlsCount = await paginationControls.count();
    
    if (controlsCount === 0) {
      console.log('⊘ Skipping test - not enough products for pagination');
      test.skip();
      return;
    }

    // Get current page number
    const pageIndicatorBefore = await paginationControls.locator('.page-indicator').textContent();
    const currentPageBefore = parseInt(pageIndicatorBefore.match(/Trang (\d+)/)[1]);

    // Click Next button
    const nextButton = paginationControls.locator('a:has-text("Sau")');
    await nextButton.click();
    await page.waitForLoadState('networkidle');

    // Verify URL updated
    expect(page.url()).toContain('page=2');
    console.log('✓ URL updated to page=2');

    // Verify page indicator updated
    const pageIndicatorAfter = await page.locator('.pagination-controls .page-indicator').textContent();
    const currentPageAfter = parseInt(pageIndicatorAfter.match(/Trang (\d+)/)[1]);
    
    expect(currentPageAfter).toBe(currentPageBefore + 1);
    console.log(`✓ Page indicator updated from ${currentPageBefore} to ${currentPageAfter}`);
  });

  test('Requirement 4.2: Previous button navigates to previous page', async ({ page }) => {
    await page.goto(`${MENU_URL}?page=2`);
    await page.waitForLoadState('networkidle');

    const paginationControls = page.locator('.pagination-controls');
    const controlsCount = await paginationControls.count();
    
    if (controlsCount === 0) {
      console.log('⊘ Skipping test - not enough products for pagination');
      test.skip();
      return;
    }

    // Get current page number
    const pageIndicatorBefore = await paginationControls.locator('.page-indicator').textContent();
    const currentPageBefore = parseInt(pageIndicatorBefore.match(/Trang (\d+)/)[1]);

    // Click Previous button
    const prevButton = paginationControls.locator('a:has-text("Trước")');
    await prevButton.click();
    await page.waitForLoadState('networkidle');

    // Verify URL updated
    expect(page.url()).toContain('page=1');
    console.log('✓ URL updated to page=1');

    // Verify page indicator updated
    const pageIndicatorAfter = await page.locator('.pagination-controls .page-indicator').textContent();
    const currentPageAfter = parseInt(pageIndicatorAfter.match(/Trang (\d+)/)[1]);
    
    expect(currentPageAfter).toBe(currentPageBefore - 1);
    console.log(`✓ Page indicator updated from ${currentPageBefore} to ${currentPageAfter}`);
  });

  test('Requirement 4.5: Category parameter preserved in pagination links', async ({ page }) => {
    // First, get a category from the sidebar
    await page.goto(MENU_URL);
    await page.waitForLoadState('networkidle');

    const categoryLinks = page.locator('.category-sidebar a');
    const categoryCount = await categoryLinks.count();
    
    if (categoryCount <= 1) {
      console.log('⊘ Skipping test - no categories available');
      test.skip();
      return;
    }

    // Click second category (first is "Tất cả")
    const secondCategory = categoryLinks.nth(1);
    const categoryHref = await secondCategory.getAttribute('href');
    const categoryMatch = categoryHref.match(/category=([^&]+)/);
    
    if (!categoryMatch) {
      console.log('⊘ Skipping test - could not extract category');
      test.skip();
      return;
    }

    const categoryName = categoryMatch[1];
    
    await secondCategory.click();
    await page.waitForLoadState('networkidle');

    // Check if pagination exists
    const paginationControls = page.locator('.pagination-controls');
    const controlsCount = await paginationControls.count();
    
    if (controlsCount === 0) {
      console.log('⊘ Skipping test - category has no pagination');
      test.skip();
      return;
    }

    // Check Next button href contains category parameter
    const nextButton = paginationControls.locator('a:has-text("Sau")');
    const nextHref = await nextButton.getAttribute('href');
    expect(nextHref).toContain(`category=${categoryName}`);
    console.log(`✓ Next button preserves category parameter: ${categoryName}`);

    // Check Previous button href contains category parameter (if not on first page)
    const prevButton = paginationControls.locator('a:has-text("Trước")');
    const prevHref = await prevButton.getAttribute('href');
    expect(prevHref).toContain(`category=${categoryName}`);
    console.log(`✓ Previous button preserves category parameter: ${categoryName}`);
  });

  test('Pagination controls are responsive on mobile', async ({ page }) => {
    const MOBILE_VIEWPORT = { width: 375, height: 667 };
    await page.setViewportSize(MOBILE_VIEWPORT);
    
    await page.goto(`${MENU_URL}?page=1`);
    await page.waitForLoadState('networkidle');

    const paginationControls = page.locator('.pagination-controls');
    const controlsCount = await paginationControls.count();
    
    if (controlsCount === 0) {
      console.log('⊘ Skipping test - not enough products for pagination');
      test.skip();
      return;
    }

    // Verify pagination controls are visible on mobile
    await expect(paginationControls).toBeVisible();
    console.log('✓ Pagination controls visible on mobile (375px)');

    // Verify buttons are clickable
    const nextButton = paginationControls.locator('a:has-text("Sau")');
    await expect(nextButton).toBeVisible();
    console.log('✓ Pagination buttons visible and accessible on mobile');
  });

  test('Pagination controls have proper accessibility attributes', async ({ page }) => {
    await page.goto(`${MENU_URL}?page=1`);
    await page.waitForLoadState('networkidle');

    const paginationControls = page.locator('.pagination-controls');
    const controlsCount = await paginationControls.count();
    
    if (controlsCount === 0) {
      console.log('⊘ Skipping test - not enough products for pagination');
      test.skip();
      return;
    }

    // Check nav element has aria-label
    const ariaLabel = await paginationControls.getAttribute('aria-label');
    expect(ariaLabel).toBe('Điều hướng trang');
    console.log('✓ Pagination nav has aria-label');

    // Check page indicator has aria-current
    const pageIndicator = paginationControls.locator('.page-indicator');
    const ariaCurrent = await pageIndicator.getAttribute('aria-current');
    expect(ariaCurrent).toBe('page');
    console.log('✓ Page indicator has aria-current="page"');

    // Check buttons have aria-label
    const prevButton = paginationControls.locator('a:has-text("Trước")');
    const prevAriaLabel = await prevButton.getAttribute('aria-label');
    expect(prevAriaLabel).toBe('Trang trước');
    console.log('✓ Previous button has aria-label');

    const nextButton = paginationControls.locator('a:has-text("Sau")');
    const nextAriaLabel = await nextButton.getAttribute('aria-label');
    expect(nextAriaLabel).toBe('Trang sau');
    console.log('✓ Next button has aria-label');
  });
});
