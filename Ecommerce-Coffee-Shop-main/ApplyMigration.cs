using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SaleStore.Data;
using Microsoft.Extensions.Configuration;

namespace SaleStore
{
    public class ApplyMigration
    {
        public static async Task Main(string[] args)
        {
            Console.WriteLine("Applying database migration: AddProductCategoryIndexes");
            
            // Load configuration
            var configuration = new ConfigurationBuilder()
                .SetBasePath(AppContext.BaseDirectory)
                .AddJsonFile("appsettings.json", optional: false)
                .Build();

            var connectionString = configuration.GetConnectionString("DefaultConnection");
            
            // Create DbContext
            var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
            optionsBuilder.UseNpgsql(connectionString)
                          .UseSnakeCaseNamingConvention();

            using var context = new ApplicationDbContext(optionsBuilder.Options);
            
            try
            {
                // Apply pending migrations
                Console.WriteLine("Applying migrations...");
                await context.Database.MigrateAsync();
                Console.WriteLine("✓ Migrations applied successfully!");
                
                // Verify indexes exist
                var sql = @"
                    SELECT indexname 
                    FROM pg_indexes 
                    WHERE tablename = 'products' 
                    AND indexname IN ('IX_Products_Category', 'IX_Products_IsActive_Category')
                    ORDER BY indexname;
                ";
                
                var connection = context.Database.GetDbConnection();
                await connection.OpenAsync();
                
                using var command = connection.CreateCommand();
                command.CommandText = sql;
                
                Console.WriteLine("\nVerifying indexes:");
                using var reader = await command.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    Console.WriteLine($"  ✓ {reader.GetString(0)}");
                }
                
                Console.WriteLine("\n✓ Migration completed successfully!");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"✗ Error applying migration: {ex.Message}");
                Console.WriteLine($"  {ex.StackTrace}");
                Environment.Exit(1);
            }
        }
    }
}
