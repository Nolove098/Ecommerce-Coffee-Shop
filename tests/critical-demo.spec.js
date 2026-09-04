// @ts-check
const { test, expect } = require('./fixtures');

const adminLoginId = process.env.BootstrapAdmin__Username || process.env.BootstrapAdmin__Email;
const adminPassword = process.env.BootstrapAdmin__Password;
const staffLoginId = process.env.BootstrapStaff__Username || process.env.BootstrapStaff__Email;
const staffPassword = process.env.BootstrapStaff__Password;
const applicationOrigin = new URL(process.env.PLAYWRIGHT_BASE_URL || 'http://localhost:5005').origin;

async function reduceExternalNoise(page) {
  await page.route('**/*', route => {
    const type = route.request().resourceType();
    return type === 'image' || type === 'font' ? route.abort() : route.continue();
  });
}

async function login(page, loginId, password, expectedRole) {
  await page.goto('/Auth/Login', { waitUntil: 'domcontentloaded' });
  expect(new URL(page.url()).origin).toBe(applicationOrigin);
  await page.locator('[name="LoginId"]').fill(loginId);
  await page.locator('[name="Password"]').fill(password);
  const [navigationResponse] = await Promise.all([
    page.waitForNavigation({ waitUntil: 'domcontentloaded' }),
    page.locator('form button[type="submit"]').click()
  ]);

  expect(navigationResponse, 'Login form should produce a navigation response').not.toBeNull();
  expect(navigationResponse.status(), 'Login navigation should not return an HTTP error').toBeLessThan(400);
  expect(new URL(page.url()).origin, 'Login must remain on the configured application host').toBe(applicationOrigin);

  const remainedOnLogin = new URL(page.url()).pathname === '/Auth/Login';
  if (remainedOnLogin) {
    const validationErrorVisible = await page.locator('.validation-summary-errors, .alert-danger')
      .filter({ hasText: /.+/ })
      .isVisible()
      .catch(() => false);
    expect(validationErrorVisible, 'A failed login should display a validation error').toBeTruthy();
  }
  expect(remainedOnLogin, 'Login should redirect away from /Auth/Login').toBeFalsy();

  // Secondary diagnostic only. Authentication is proved by CurrentUser below.
  const authCookie = (await page.context().cookies())
    .find(cookie => cookie.httpOnly && !cookie.name.toLowerCase().includes('antiforgery'));
  const hasHttpOnlyCookie = Boolean(authCookie);
  expect(authCookie, 'Authentication cookie should be HttpOnly').toBeTruthy();
  if (applicationOrigin.startsWith('https://')) {
    expect(authCookie.secure, 'Production authentication cookie should be Secure').toBe(true);
  }
  test.info().annotations.push({
    type: 'authentication cookie enumerated',
    description: hasHttpOnlyCookie ? 'YES' : 'NO'
  });

  // page.request is the APIRequestContext owned by this BrowserContext, so it
  // sends the same cookie jar as the page in the installed Playwright version.
  const currentUser = await page.context().request.get('/Auth/CurrentUser');
  expect(currentUser.status(), 'Authentication cookie should be accepted by the server').toBe(200);
  expect(currentUser.headers()['content-type'], 'CurrentUser must return JSON')
    .toMatch(/^application\/json\b/i);
  const identity = await currentUser.json();
  expect(identity.authenticated).toBe(true);
  expect(identity.role).toBe(expectedRole);

  return identity;
}

test.describe.serial('critical recruiter/demo flows', () => {
  test('public pages and protected-route behavior', async ({ page }) => {
    await reduceExternalNoise(page);
    await page.goto('/', { waitUntil: 'domcontentloaded' });
    await expect(page.locator('button[onclick*="addToCart"]').first()).toBeVisible();

    const recommendations = await page.context().request.get('/api/Ai/recommend/ml');
    expect(recommendations.status()).toBe(200);
    const recommendationBody = await recommendations.json();
    expect(recommendationBody.recommendations.length).toBeGreaterThan(0);

    await page.goto('/Admin/Dashboard', { waitUntil: 'domcontentloaded' });
    expect(new URL(page.url()).pathname).toBe('/Auth/Login');
    await page.goto('/Staff/POS', { waitUntil: 'domcontentloaded' });
    expect(new URL(page.url()).pathname).toBe('/Auth/Login');
  });

  test('customer register, login, cart, COD checkout, and history', async ({ page }) => {
    test.setTimeout(90000);
    await reduceExternalNoise(page);
    const marker = `${Date.now()}${Math.floor(Math.random() * 10000)}`;
    const email = `phase2-critical-${marker}@example.test`;
    const password = `P2!${marker}aA`;
    const phone = `08${marker.slice(-8)}`;

    await page.goto('/Auth/Register', { waitUntil: 'domcontentloaded' });
    await page.locator('[name="FullName"]').fill('Phase Two Critical');
    await page.locator('[name="Email"]').fill(email);
    await page.locator('[name="Phone"]').fill(phone);
    await page.locator('[name="Password"]').fill(password);
    await page.locator('[name="ConfirmPassword"]').fill(password);
    await Promise.all([
      page.waitForURL(url => url.pathname === '/', { waitUntil: 'domcontentloaded' }),
      page.locator('form button[type="submit"]').click()
    ]);

    const logout = page.getByRole('button', { name: /xuất/i }).first();
    await Promise.all([
      page.waitForURL(url => url.pathname === '/', { waitUntil: 'domcontentloaded' }),
      logout.click()
    ]);
    await login(page, email, password, 'User');

    const negotiate = await page.request.post('/hubs/order/negotiate?negotiateVersion=1');
    expect(negotiate.status()).toBe(200);

    await page.locator('button[onclick*="addToCart"]').first().click();
    await expect.poll(async () => page.evaluate(() => CartSync.getCart().length)).toBeGreaterThan(0);
    await expect.poll(async () => (await (await page.request.get('/api/cart')).json()).length).toBeGreaterThan(0);

    await page.goto('/Cart/Checkout', { waitUntil: 'domcontentloaded' });
    await page.locator('[name="CustomerName"]').fill('Phase Two Critical');
    await page.locator('[name="CustomerPhone"]').fill(phone);
    await page.locator('[name="Address"]').fill('Phase 2 critical validation');
    await page.locator('[name="PaymentMethod"][value="COD"]').check();
    await Promise.all([
      page.waitForURL(url => /\/Cart\/Success\/\d+$/.test(url.pathname), { waitUntil: 'domcontentloaded' }),
      page.locator('#placeOrderBtn').click()
    ]);
    await expect(page.locator('body')).toContainText('COD');

    await page.goto('/Order/History', { waitUntil: 'domcontentloaded' });
    await expect(page.locator('[data-realtime-order-id]').first()).toBeVisible();
  });

  test('Gemini responds', async ({ request }) => {
    const response = await request.post('/api/ChatBot', {
      data: { messages: [{ role: 'user', content: 'Recommend one coffee in one short sentence.' }] }
    });
    expect(response.status()).toBe(200);
    expect((await response.json()).reply).toBeTruthy();
  });

  test('authenticated Admin and Staff pages', async ({ browser }) => {
    test.skip(!adminLoginId || !adminPassword || !staffLoginId || !staffPassword,
      'Bootstrap Admin/Staff environment configuration is required.');

    const adminPage = await browser.newPage();
    await reduceExternalNoise(adminPage);
    await login(adminPage, adminLoginId, adminPassword, 'Admin');
    await adminPage.goto('/Admin/Dashboard', { waitUntil: 'domcontentloaded' });
    await expect(adminPage).toHaveURL(/\/Admin\/Dashboard/);
    await adminPage.goto('/Admin/Product', { waitUntil: 'domcontentloaded' });
    await expect(adminPage).toHaveURL(/\/Admin\/Product/);
    await adminPage.goto('/Admin/Order', { waitUntil: 'domcontentloaded' });
    await expect(adminPage).toHaveURL(/\/Admin\/Order/);

    const forecast = await adminPage.context().request.get('/api/Ai/admin/forecast?days=7');
    expect(forecast.status()).toBe(200);
    expect(forecast.headers()['content-type']).toMatch(/^application\/json\b/i);
    const forecastBody = await forecast.json();
    expect(forecastBody.error).toBeNull();
    expect(forecastBody.historical.length).toBeGreaterThanOrEqual(7);
    expect(forecastBody.forecast).toHaveLength(7);

    const insights = await adminPage.context().request.get('/api/Ai/admin/insights');
    expect(insights.status()).toBe(200);
    expect((await insights.json()).insights).toBeTruthy();

    const staffPage = await browser.newPage();
    await reduceExternalNoise(staffPage);
    await login(staffPage, staffLoginId, staffPassword, 'Staff');
    await staffPage.goto('/Staff/POS', { waitUntil: 'domcontentloaded' });
    await expect(staffPage).toHaveURL(/\/Staff\/POS/);
    await expect(staffPage.locator('body')).toContainText(/POS|bán hàng/i);
  });

  test('private customer SignalR update occurs without page reload', async ({ browser }) => {
    test.skip(!adminLoginId || !adminPassword, 'Bootstrap Admin environment configuration is required.');
    test.setTimeout(120000);

    const customer = await browser.newPage();
    await reduceExternalNoise(customer);
    const marker = `${Date.now()}${Math.floor(Math.random() * 10000)}`;
    const email = `phase2-signalr-${marker}@example.test`;
    const password = `P2!${marker}aA`;
    const phone = `07${marker.slice(-8)}`;

    await customer.goto('/Auth/Register', { waitUntil: 'domcontentloaded' });
    await customer.locator('[name="FullName"]').fill('Phase Two SignalR');
    await customer.locator('[name="Email"]').fill(email);
    await customer.locator('[name="Phone"]').fill(phone);
    await customer.locator('[name="Password"]').fill(password);
    await customer.locator('[name="ConfirmPassword"]').fill(password);
    await Promise.all([
      customer.waitForURL(url => url.pathname === '/', { waitUntil: 'domcontentloaded' }),
      customer.locator('form button[type="submit"]').click()
    ]);
    await customer.locator('button[onclick*="addToCart"]').first().click();
    await expect.poll(async () => customer.evaluate(() => CartSync.getCart().length)).toBeGreaterThan(0);
    await customer.goto('/Cart/Checkout', { waitUntil: 'domcontentloaded' });
    await customer.locator('[name="CustomerName"]').fill('Phase Two SignalR');
    await customer.locator('[name="CustomerPhone"]').fill(phone);
    await customer.locator('[name="PaymentMethod"][value="COD"]').check();
    await Promise.all([
      customer.waitForURL(url => /\/Cart\/Success\/\d+$/.test(url.pathname), { waitUntil: 'domcontentloaded' }),
      customer.locator('#placeOrderBtn').click()
    ]);
    const orderId = customer.url().match(/\/Cart\/Success\/(\d+)/)[1];
    await customer.goto(`/Order/Detail/${orderId}`, { waitUntil: 'domcontentloaded' });
    await expect(customer.locator('[data-order-status]')).toHaveAttribute('data-order-status', 'Pending');

    const admin = await browser.newPage();
    await reduceExternalNoise(admin);
    await login(admin, adminLoginId, adminPassword, 'Admin');
    await admin.goto('/Admin/Order', { waitUntil: 'domcontentloaded' });
    const unrelatedOrderHref = await admin.locator('a[href*="/Admin/Order/Detail/"]')
      .evaluateAll((links, currentOrderId) => links
        .map(link => link.getAttribute('href'))
        .find(href => href && !href.endsWith(`/${currentOrderId}`)), orderId);

    if (unrelatedOrderHref) {
      const unrelatedOrderId = unrelatedOrderHref.match(/\/Detail\/(\d+)/)?.[1];
      expect(unrelatedOrderId).toBeTruthy();
      const unrelatedJoinDenied = await customer.evaluate(async id => {
        const connection = new signalR.HubConnectionBuilder().withUrl('/hubs/order').build();
        try {
          await connection.start();
          await connection.invoke('JoinOrderGroup', id);
          return false;
        } catch {
          return true;
        } finally {
          await connection.stop().catch(() => {});
        }
      }, unrelatedOrderId);
      expect(unrelatedJoinDenied, 'A customer must not join another order private group').toBe(true);
    }

    const detailResponse = await admin.goto(`/Admin/Order/Detail/${orderId}`, { waitUntil: 'domcontentloaded' });
    expect(detailResponse?.status(), 'Admin should be able to load the new order').toBe(200);
    await expect(admin).toHaveURL(new RegExp(`/Admin/Order/Detail/${orderId}$`));

    const statusSelect = admin.locator('select.form-select').filter({ has: admin.locator('option[value="Ready"]') });
    const updateButton = admin.locator('button.btn-coffee');
    await statusSelect.selectOption('Ready');
    await Promise.all([
      admin.waitForURL(new RegExp(`/Admin/Order/Detail/${orderId}$`), { waitUntil: 'domcontentloaded' }),
      updateButton.click()
    ]);
    await expect(customer.locator('[data-order-status]')).toHaveAttribute('data-order-status', 'Ready', { timeout: 15000 });
  });
});
