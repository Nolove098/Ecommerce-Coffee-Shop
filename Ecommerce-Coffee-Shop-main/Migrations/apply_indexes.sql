-- Migration: AddProductCategoryIndexes
-- This script adds performance indexes to the products table

-- Create index on category column for category filtering queries
CREATE INDEX IF NOT EXISTS "IX_Products_Category" ON products (category);

-- Create composite index on is_active and category for filtered category queries
CREATE INDEX IF NOT EXISTS "IX_Products_IsActive_Category" ON products (is_active, category);

-- Verify indexes were created
SELECT 
    schemaname,
    tablename,
    indexname,
    indexdef
FROM pg_indexes
WHERE tablename = 'products'
ORDER BY indexname;
