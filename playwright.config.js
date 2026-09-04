// @ts-check
const { defineConfig, devices } = require('@playwright/test');
const baseURL = process.env.PLAYWRIGHT_BASE_URL || 'http://localhost:5005';
const workers = Number.parseInt(process.env.PLAYWRIGHT_WORKERS || '1', 10);

/**
 * Playwright configuration for responsive behavior testing
 * Tests Bootstrap breakpoints: 576px, 768px, 992px, 1200px, 1400px
 */
module.exports = defineConfig({
  // Hosted database state is shared, so local Phase 2 validation is serial by
  // default. PLAYWRIGHT_WORKERS can be raised after the suite proves parallel-safe.
  testDir: './tests',
  fullyParallel: false,
  forbidOnly: !!process.env.CI,
  retries: process.env.CI ? 2 : 0,
  workers: Number.isFinite(workers) && workers > 0 ? workers : 1,
  reporter: [
    ['html'],
    ['list']
  ],
  use: {
    baseURL,
    navigationTimeout: 15000,
    trace: 'on-first-retry',
    screenshot: 'only-on-failure',
  },

  projects: [
    {
      name: 'chromium',
      use: { ...devices['Desktop Chrome'] },
    },
  ],

  // Start the application only for local tests. Hosted validation targets the
  // explicitly supplied URL and must not start a second local process.
  webServer: process.env.PLAYWRIGHT_BASE_URL ? undefined : {
    command: 'dotnet run --project ./SaleStore.csproj',
    url: baseURL,
    reuseExistingServer: !process.env.CI,
    timeout: 120 * 1000,
  },
});
