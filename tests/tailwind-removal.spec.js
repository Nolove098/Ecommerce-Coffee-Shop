// @ts-check
const { test, expect } = require('@playwright/test');
const fs = require('fs');
const path = require('path');

/**
 * Tailwind CSS Removal Validation (Task 15.3)
 * Verifies that no Tailwind CSS remains in the codebase
 */

test.describe('Tailwind CSS Removal Validation', () => {
  
  const viewFiles = [
    'Views/Auth/Login.cshtml',
    'Views/Auth/Register.cshtml',
    'Views/Home/Index.cshtml',
    'Views/Cart/Checkout.cshtml',
    'Views/Cart/Success.cshtml',
    'Views/Product/Detail.cshtml',
    'Views/Product/Category.cshtml',
    'Areas/Admin/Views/Shared/_AdminLayout.cshtml',
    'Areas/Admin/Views/Shared/_AdminSidebar.cshtml',
    'Areas/Admin/Views/Dashboard/Index.cshtml',
    'Areas/Admin/Views/Product/Index.cshtml',
    'Areas/Admin/Views/Product/Create.cshtml',
    'Areas/Admin/Views/Product/Edit.cshtml',
    'Areas/Admin/Views/Product/Delete.cshtml',
    'Areas/Admin/Views/Order/Index.cshtml',
    'Areas/Admin/Views/Order/Detail.cshtml',
    'Areas/Admin/Views/UserManagement/Index.cshtml',
    'Areas/Admin/Views/UserManagement/CreateStaff.cshtml',
    'Areas/Staff/Views/Shared/_POSLayout.cshtml',
    'Areas/Staff/Views/POS/Index.cshtml'
  ];

  test('15.3.1 - No Tailwind CDN references in any view file', () => {
    const tailwindCDNPatterns = [
      /cdn\.tailwindcss\.com/,
      /tailwindcss@/,
      /tailwind\.css/
    ];

    viewFiles.forEach(file => {
      const filePath = path.join(process.cwd(), file);
      if (fs.existsSync(filePath)) {
        const content = fs.readFileSync(filePath, 'utf-8');
        
        tailwindCDNPatterns.forEach(pattern => {
          expect(content).not.toMatch(pattern);
        });
      }
    });
  });

  test('15.3.2 - No Tailwind grid classes (grid-cols-*)', () => {
    const tailwindGridPattern = /\bgrid-cols-\d+\b/;

    viewFiles.forEach(file => {
      const filePath = path.join(process.cwd(), file);
      if (fs.existsSync(filePath)) {
        const content = fs.readFileSync(filePath, 'utf-8');
        expect(content).not.toMatch(tailwindGridPattern);
      }
    });
  });

  test('15.3.3 - No Tailwind text sizing classes (text-sm, text-lg, text-xl, text-2xl, text-3xl)', () => {
    // Note: Bootstrap uses 'small' class and fs-* classes instead
    const tailwindTextSizePattern = /\btext-(sm|lg|xl|2xl|3xl|4xl|5xl|6xl)\b/;

    viewFiles.forEach(file => {
      const filePath = path.join(process.cwd(), file);
      if (fs.existsSync(filePath)) {
        const content = fs.readFileSync(filePath, 'utf-8');
        expect(content).not.toMatch(tailwindTextSizePattern);
      }
    });
  });

  test('15.3.4 - No Tailwind flex direction classes (flex-row, flex-col)', () => {
    // Bootstrap uses flex-row and flex-column instead
    const tailwindFlexPattern = /\bflex-col\b/;

    viewFiles.forEach(file => {
      const filePath = path.join(process.cwd(), file);
      if (fs.existsSync(filePath)) {
        const content = fs.readFileSync(filePath, 'utf-8');
        expect(content).not.toMatch(tailwindFlexPattern);
      }
    });
  });

  test('15.3.5 - No Tailwind alignment classes (items-*, justify-*)', () => {
    // Bootstrap uses align-items-* and justify-content-* instead
    // Tailwind uses: items-center, items-start, justify-between, justify-center (without prefixes)
    // Bootstrap uses: align-items-center, justify-content-between (with prefixes)
    const tailwindAlignmentPatterns = [
      /\bclass="[^"]*\s+items-(start|end|center|baseline|stretch)\b/,
      /\bclass="[^"]*\s+justify-(start|end|center|between|around|evenly)\b(?!\-content)/
    ];

    viewFiles.forEach(file => {
      const filePath = path.join(process.cwd(), file);
      if (fs.existsSync(filePath)) {
        const content = fs.readFileSync(filePath, 'utf-8');
        
        tailwindAlignmentPatterns.forEach(pattern => {
          expect(content).not.toMatch(pattern);
        });
      }
    });
  });

  test('15.3.6 - No Tailwind arbitrary values (square brackets)', () => {
    // Tailwind uses arbitrary values like bg-[#6F4E37], text-[14px]
    // Bootstrap uses CSS variables or custom classes instead
    const tailwindArbitraryPattern = /class="[^"]*\[[^\]]+\][^"]*"/;

    viewFiles.forEach(file => {
      const filePath = path.join(process.cwd(), file);
      if (fs.existsSync(filePath)) {
        const content = fs.readFileSync(filePath, 'utf-8');
        
        // Allow style attributes with brackets (inline styles are OK)
        const classMatches = content.match(/class="[^"]*"/g) || [];
        classMatches.forEach(match => {
          expect(match).not.toMatch(/\[[^\]]+\]/);
        });
      }
    });
  });

  test('15.3.7 - All views use Bootstrap classes', async ({ page }) => {
    // Verify that Bootstrap classes are present and working
    await page.goto('/');
    
    // Check for Bootstrap-specific classes
    const bootstrapClasses = [
      '.container',
      '.row',
      '.col',
      '.btn',
      '.card',
      '.navbar'
    ];

    for (const selector of bootstrapClasses) {
      const element = page.locator(selector).first();
      await expect(element).toBeAttached();
    }
  });

  test('15.3.8 - No Tailwind config file exists', () => {
    const tailwindConfigFiles = [
      'tailwind.config.js',
      'tailwind.config.cjs',
      'tailwind.config.ts'
    ];

    tailwindConfigFiles.forEach(file => {
      const filePath = path.join(process.cwd(), file);
      expect(fs.existsSync(filePath)).toBe(false);
    });
  });
});
