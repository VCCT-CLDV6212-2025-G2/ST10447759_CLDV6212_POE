/*
 * ST10447759
 * CLDV6212
 * Part 3
 */
using AzureRetailHub.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace AzureRetailHub.Data
{
    // Inherit from IdentityDbContext to get all the User/Role tables
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        // Add our custom models to the database context
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

            // Configure composite primary key for OrderItem if needed
            // (though int Id is simpler)
        }
    }
}