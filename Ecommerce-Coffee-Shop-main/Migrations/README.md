# Database Migrations

This folder contains Entity Framework Core migrations for the SaleStore application.

## Migration: AddProductCategoryIndexes (20260331221159)

### Purpose
Adds database indexes to optimize query performance for category filtering and pagination on the Menu page.

### Changes
1. **Index on Products.Category**: Speeds up category filtering queries
2. **Composite Index on Products.IsActive + Category**: Optimizes queries that filter by both active status and category

### Database Impact
- **Table**: `products`
- **Indexes Added**:
  - `IX_Products_Category` on `category` column
  - `IX_Products_IsActive_Category` on `(is_active, category)` columns

### Performance Benefits
- Faster category sidebar loading (category statistics query)
- Faster product filtering by category
- Improved pagination query performance
- Reduced database load for menu page requests

### How to Apply

#### Automatic (Recommended)
The migration is automatically applied when the application starts via `Program.cs`:
```csharp
await dbContext.Database.MigrateAsync();
```

Simply restart the application and the indexes will be created.

#### Manual SQL (Alternative)
If you prefer to apply manually, run:
```bash
psql -h <host> -p 5432 -U <username> -d <database> -f Migrations/apply_indexes.sql
```

Or execute the SQL directly:
```sql
CREATE INDEX IF NOT EXISTS "IX_Products_Category" ON products (category);
CREATE INDEX IF NOT EXISTS "IX_Products_IsActive_Category" ON products (is_active, category);
```

### Rollback
To remove these indexes:
```sql
DROP INDEX IF EXISTS "IX_Products_Category";
DROP INDEX IF EXISTS "IX_Products_IsActive_Category";
```

### Notes
- Uses PostgreSQL snake_case naming convention (via EFCore.NamingConventions)
- Indexes use `IF NOT EXISTS` to prevent errors on re-application
- No data migration required - only schema changes
