-- Script to verify Product table indexes exist
-- Run this after restarting the application to confirm migration was applied

SELECT 
    schemaname,
    tablename,
    indexname,
    indexdef
FROM pg_indexes
WHERE tablename = 'products'
ORDER BY indexname;

-- Expected output should include:
-- IX_Products_Category
-- IX_Products_IsActive_Category
