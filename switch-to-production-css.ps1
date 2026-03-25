# Switch to Production CSS (custom.min.css)
# This script updates all layout files to use the minified CSS for production deployment

Write-Host "Switching to Production CSS (custom.min.css)..." -ForegroundColor Cyan
Write-Host ""

$files = @(
    "Areas/Admin/Views/Shared/_AdminLayout.cshtml",
    "Areas/Staff/Views/Shared/_POSLayout.cshtml",
    "Views/Auth/Login.cshtml",
    "Views/Auth/Register.cshtml",
    "Views/Home/Index.cshtml",
    "Views/Cart/Checkout.cshtml",
    "Views/Cart/Success.cshtml",
    "Views/Product/Detail.cshtml",
    "Views/Product/Category.cshtml"
)

$updatedCount = 0
$skippedCount = 0

foreach ($file in $files) {
    if (Test-Path $file) {
        $content = Get-Content $file -Raw
        
        # Check if already using minified version
        if ($content -match 'custom\.min\.css') {
            Write-Host "  [SKIP] $file (already using custom.min.css)" -ForegroundColor Yellow
            $skippedCount++
        }
        # Check if using development version
        elseif ($content -match 'custom\.css') {
            # Replace custom.css with custom.min.css
            $newContent = $content -replace 'custom\.css', 'custom.min.css'
            Set-Content $file -Value $newContent -NoNewline
            Write-Host "  [OK] $file" -ForegroundColor Green
            $updatedCount++
        }
        else {
            Write-Host "  [WARN] $file (no custom.css reference found)" -ForegroundColor Yellow
            $skippedCount++
        }
    }
    else {
        Write-Host "  [ERROR] $file (file not found)" -ForegroundColor Red
    }
}

Write-Host ""
Write-Host "Summary:" -ForegroundColor Cyan
Write-Host "  Updated: $updatedCount files" -ForegroundColor Green
Write-Host "  Skipped: $skippedCount files" -ForegroundColor Yellow
Write-Host ""
Write-Host "Production CSS switch complete!" -ForegroundColor Green
