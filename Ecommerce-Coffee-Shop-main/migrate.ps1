# PowerShell script to apply EF Core migrations
Write-Host "Applying database migrations..." -ForegroundColor Cyan

# Try using dotnet ef
$result = dotnet ef database update --verbose 2>&1

if ($LASTEXITCODE -eq 0) {
    Write-Host "✓ Migrations applied successfully!" -ForegroundColor Green
} else {
    Write-Host "✗ Failed to apply migrations using dotnet ef" -ForegroundColor Red
    Write-Host "Error: $result" -ForegroundColor Red
    Write-Host ""
    Write-Host "Alternative: Run the SQL script manually:" -ForegroundColor Yellow
    Write-Host "  psql -h aws-1-ap-northeast-2.pooler.supabase.com -p 5432 -U postgres.uxlkglkfpmtpvruqndqx -d postgres -f Migrations/apply_indexes.sql" -ForegroundColor Yellow
    Write-Host ""
    Write-Host "Or apply the migration when the application starts by calling:" -ForegroundColor Yellow
    Write-Host "  await dbContext.Database.MigrateAsync();" -ForegroundColor Yellow
}
