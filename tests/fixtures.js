const base = require('@playwright/test');

// Keep application navigation independent of third-party CDN load events.
const test = base.test.extend({
  page: async ({ page }, use) => {
    const goto = page.goto.bind(page);
    const waitForLoadState = page.waitForLoadState.bind(page);
    page.goto = (url, options = {}) => goto(url, {
      waitUntil: 'domcontentloaded',
      ...options
    });
    page.waitForLoadState = (state = 'load', options) =>
      waitForLoadState(state === 'networkidle' ? 'domcontentloaded' : state, options);
    await use(page);
  }
});

module.exports = { test, expect: base.expect };
