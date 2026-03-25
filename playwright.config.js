// @ts-check
const { defineConfig, devices } = require('@playwright/test');

/**
 * Playwright configuration for responsive behavior testing
 * Tests Bootstrap breakpoints: 576px, 768px, 992px, 1200px, 1400px
 */
module.exports = defineConfig({
  testDir: './tests',
  fullyParallel: true,
  forbidOnly: !!process.env.CI,
  retries: process.env.CI ? 2 : 0,
  workers: process.env.CI ? 1 : undefined,
  reporter: [
    ['html'],
    ['list']
  ],
  use: {
    baseURL: 'http://localhost:5005',
    trace: 'on-first-retry',
    screenshot: 'only-on-failure',
  },

  projects: [
    {
      name: 'chromium',
      use: { ...devices['Desktop Chrome'] },
    },
  ],

  // Run local dev server before starting tests
  webServer: {
    command: 'dotnet run',
    url: 'http://localhost:5005',
    reuseExistingServer: !process.env.CI,
    timeout: 120 * 1000,
  },
});
