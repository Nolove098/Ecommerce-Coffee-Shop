# Responsive Behavior Testing

This directory contains automated and manual tests for verifying responsive behavior at all Bootstrap breakpoints.

## Task 12.1: Verify Responsive Behavior at All Bootstrap Breakpoints

**Requirements:** 12.1, 12.2, 12.3, 19.3

This task validates that all pages in the SaleStore application respond correctly at Bootstrap 5 standard breakpoints:
- **576px (sm)** - Small devices (landscape phones)
- **768px (md)** - Medium devices (tablets)
- **992px (lg)** - Large devices (desktops)
- **1200px (xl)** - Extra large devices (large desktops)
- **1400px (xxl)** - Extra extra large devices (larger desktops)

## Quick Start

### Prerequisites

- Node.js 16+ installed
- .NET 6.0 SDK installed
- SaleStore application running on `http://localhost:5000`

### Installation

```bash
# Install Node.js dependencies
npm install

# Install Playwright browsers
npx playwright install
```

### Running Tests

```bash
# Run all responsive tests
npm test

# Run tests with UI mode (recommended for debugging)
npm run test:ui

# Run tests in headed mode (see browser)
npm run test:headed

# Run only responsive breakpoint tests
npm run test:responsive
```

## Test Files

### Automated Tests

- **`responsive-breakpoints.spec.js`** - Comprehensive automated tests for all breakpoints
  - Tests all public pages (Home, Login, Register, Checkout, etc.)
  - Tests responsive grid layouts
  - Tests responsive utilities (d-none, d-md-block, etc.)
  - Tests images and tables
  - Tests Bootstrap components (navbar, cards, etc.)
  - Validates no console errors
  - Validates no horizontal overflow

### Manual Testing

- **`RESPONSIVE_TESTING_CHECKLIST.md`** - Detailed manual testing checklist
  - Step-by-step testing instructions
  - Comprehensive validation criteria
  - Test results template
  - Common issues to check

## Test Coverage

### Pages Tested

#### Public Area
- Home page (/)
- Login page (/Auth/Login)
- Register page (/Auth/Register)
- Checkout page (/Cart/Checkout)
- Product Detail page (/Product/Detail)
- Product Category page (/Product/Category)

#### Admin Area (requires authentication)
- Admin Dashboard (/Admin/Dashboard)
- Admin Products (/Admin/Product)
- Admin Product Create (/Admin/Product/Create)
- Admin Orders (/Admin/Order)
- Admin User Management (/Admin/UserManagement)

#### Staff Area (requires authentication)
- Staff POS (/Staff/POS)

### Validation Checks

For each page at each breakpoint, the tests verify:

1. **No horizontal overflow** - Page content fits within viewport
2. **Responsive navigation** - Navbar collapses at mobile breakpoints
3. **Grid layouts** - Bootstrap grid adapts correctly
4. **Responsive utilities** - Display classes work (d-none, d-md-block)
5. **Images** - Scale properly without overflow
6. **Tables** - Wrapped in table-responsive
7. **Bootstrap components** - Maintain structure and functionality
8. **No console errors** - JavaScript loads without errors

## Test Results

After running tests, view the HTML report:

```bash
npx playwright show-report
```

The report includes:
- Test pass/fail status
- Screenshots of failures
- Detailed error messages
- Execution time

## Troubleshooting

### Application Not Running

If tests fail with connection errors:

1. Start the application manually:
   ```bash
   dotnet run
   ```

2. Verify it's running on `http://localhost:5000`

3. Run tests again

### Port Conflicts

If port 5000 is in use, update `playwright.config.js`:

```javascript
use: {
  baseURL: 'http://localhost:YOUR_PORT',
},
webServer: {
  command: 'dotnet run',
  url: 'http://localhost:YOUR_PORT',
}
```

### Browser Installation Issues

If Playwright browsers fail to install:

```bash
# Install with sudo (Linux/Mac)
sudo npx playwright install

# Or install specific browser
npx playwright install chromium
```

### Test Failures

If tests fail:

1. Check the HTML report for details
2. Run tests in headed mode to see what's happening:
   ```bash
   npm run test:headed
   ```
3. Check the manual testing checklist for specific issues
4. Review screenshots in `test-results/` directory

## Manual Testing

If automated tests cannot run, use the manual testing checklist:

1. Open `RESPONSIVE_TESTING_CHECKLIST.md`
2. Follow the step-by-step instructions
3. Test each page at each breakpoint
4. Document results in the template
5. Report any issues found

## CI/CD Integration

To run tests in CI/CD pipeline:

```yaml
# Example GitHub Actions workflow
- name: Install dependencies
  run: npm ci

- name: Install Playwright browsers
  run: npx playwright install --with-deps

- name: Run responsive tests
  run: npm test

- name: Upload test results
  uses: actions/upload-artifact@v3
  if: always()
  with:
    name: playwright-report
    path: playwright-report/
```

## Contributing

When adding new pages or components:

1. Add the page URL to `PAGES` object in `responsive-breakpoints.spec.js`
2. Add specific tests for new components
3. Update the manual testing checklist
4. Run tests to ensure they pass

## Support

For issues or questions:
- Check the manual testing checklist
- Review Playwright documentation: https://playwright.dev
- Check Bootstrap responsive documentation: https://getbootstrap.com/docs/5.3/layout/breakpoints/

## License

This testing suite is part of the SaleStore Tailwind to Bootstrap migration project.
