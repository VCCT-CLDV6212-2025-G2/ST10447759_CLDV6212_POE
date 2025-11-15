using Microsoft.EntityFrameworkCore;

namespace AzureRetailHub.Functions.Data
{
    // Note: This does NOT inherit from IdentityDbContext
    public class ApplicationDbContext : DbContext
    {
        public DbSet<User> Users { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Set up the one-to-many relationship for User -> Orders
            builder.Entity<User>()
                .HasMany(u => u.Orders)
                .WithOne(o => o.User)
                .HasForeignKey(o => o.UserId);

            // Set up the one-to-many relationship for Order -> OrderItems
            builder.Entity<Order>()
                .HasMany(o => o.Items)
                .WithOne(i => i.Order)
                .HasForeignKey(i => i.OrderId);

            // Set up the one-to-many relationship for Product -> OrderItems
            builder.Entity<Product>()
                .HasMany<OrderItem>()
                .WithOne(i => i.Product)
                .HasForeignKey(i => i.ProductId);
        }
    }
}