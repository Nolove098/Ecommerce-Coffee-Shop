using SaleStore.Models; // Đảm bảo namespace này khớp với project của bạn
using Microsoft.EntityFrameworkCore;

namespace SaleStore.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        // Khai báo các bảng
        public DbSet<Customer> Customers { get; set; } = null!;
        public DbSet<Product> Products { get; set; } = null!;
        public DbSet<Order> Orders { get; set; } = null!;
        public DbSet<OrderItem> OrderItems { get; set; } = null!;
        public DbSet<AppUser> AppUsers { get; set; } = null!;
        public DbSet<AuthActivity> AuthActivities { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<AppUser>(entity =>
            {
                entity.ToTable("app_users");
                entity.Property(x => x.Username).HasMaxLength(50);
                entity.Property(x => x.FullName).HasMaxLength(120);
                entity.Property(x => x.Email).HasMaxLength(180);
                entity.Property(x => x.Phone).HasMaxLength(30);
                entity.Property(x => x.Role).HasMaxLength(20).HasDefaultValue(AppRoles.User);
                entity.Property(x => x.IsActive).HasDefaultValue(true);
                entity.HasIndex(x => x.Email).IsUnique();
                entity.HasIndex(x => x.Username).IsUnique();
            });

            modelBuilder.Entity<AuthActivity>(entity =>
            {
                entity.ToTable("auth_activities");
                entity.Property(x => x.ActivityType).HasMaxLength(32);
                entity.Property(x => x.IpAddress).HasMaxLength(64);
                entity.Property(x => x.UserAgent).HasMaxLength(512);
                entity.HasOne(x => x.User)
                      .WithMany(x => x.AuthActivities)
                      .HasForeignKey(x => x.UserId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<OrderItem>(entity =>
            {
                entity.ToTable("order_items");
                entity.Property(x => x.LineTotal)
                      .HasComputedColumnSql("unit_price * quantity", stored: true)
                      .ValueGeneratedOnAddOrUpdate();
            });

            modelBuilder.Entity<Order>(entity =>
            {
                entity.ToTable("orders");
                entity.HasOne(x => x.CreatedByUser)
                      .WithMany()
                      .HasForeignKey(x => x.CreatedByUserId)
                      .OnDelete(DeleteBehavior.SetNull);
            });
        }
    }
}