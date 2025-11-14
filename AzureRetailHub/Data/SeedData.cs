/*
 * MODIFIED FOR PART 3
 * Contains BOTH: SeedIdentityAsync + SeedProductsAsync
 */

using System.Net.Http;
using Azure.Data.Tables;
using AzureRetailHub.Models;
using AzureRetailHub.Services;
using AzureRetailHub.Settings;

using Microsoft.AspNetCore.Identity;       // required for Identity seeding
using Microsoft.Extensions.Options;

namespace AzureRetailHub.Data
{
    public static class SeedData
    {
        // ---------------------------------------------------------
        // 1. SEED IDENTITY (Admin & Customer roles + Admin account)
        // ---------------------------------------------------------
        public static async Task SeedIdentityAsync(
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager)
        {
            // Create roles if they don't exist
            if (!await roleManager.RoleExistsAsync("Admin"))
                await roleManager.CreateAsync(new IdentityRole("Admin"));

            if (!await roleManager.RoleExistsAsync("Customer"))
                await roleManager.CreateAsync(new IdentityRole("Customer"));

            // Admin user details
            var adminEmail = "admin@abcretail.com";

            if (await userManager.FindByEmailAsync(adminEmail) == null)
            {
                var adminUser = new ApplicationUser
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    FullName = "Admin User",
                    EmailConfirmed = true
                };

                // Default admin password (change for production)
                var result = await userManager.CreateAsync(adminUser, "Pa$$w0rd");

                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(adminUser, "Admin");
                }
            }
        }


        // ---------------------------------------------------------
        // 2. SEED PRODUCTS TO TABLE STORAGE + BLOB STORAGE
        // ---------------------------------------------------------

        private static readonly (string name, string desc, decimal price, string imageUrl)[] Samples =
        {
            ("Cozy Knit Sweater", "Soft warm sweater", 79.99m, "https://picsum.photos/seed/p1/800/600"),
            ("Leather Ankle Boots", "Stylish leather boots", 129.99m, "https://picsum.photos/seed/p2/800/600"),
            ("Classic Trench Coat", "Timeless outerwear", 149.99m, "httpsum.photos/seed/p3/800/600"),
            ("Cashmere Scarf", "Luxurious scarf", 49.99m, "https://picsum.photos/seed/p4/800/600"),
            ("Slim Fit Jeans", "Comfort stretch denim", 59.99m, "https://picsum.photos/seed/p5/800/600")
        };

        public static async Task SeedProductsAsync(
            TableStorageService table,
            BlobStorageService blob,
            StorageOptions opts,
            bool force = false)
        {
            // Check if table already contains items
            var any = false;
            await foreach (var e in table.QueryEntitiesAsync(opts.ProductsTable))
            {
                any = true;
                break;
            }

            // Skip seeding if products exist unless force=true
            if (any && !force)
                return;

            using var http = new HttpClient();

            // Iterate through sample product list
            for (int i = 0; i < Samples.Length; i++)
            {
                var s = Samples[i];

                // Download image from URL
                var imgBytes = await http.GetByteArrayAsync(s.imageUrl);
                using var ms = new MemoryStream(imgBytes);

                // Generate unique blob name
                var blobName = $"seed-{i}-{Guid.NewGuid()}.jpg";

                // Upload to Blob Storage
                var blobUrl = await blob.UploadFileAsync(
                    opts.BlobContainer,
                    blobName,
                    ms);

                // Create product DTO
                var product = new Product
                {
                    Id = Guid.NewGuid().ToString(),
                    Name = s.name,
                    Description = s.desc,
                    Price = s.price,
                    ImageUrl = blobUrl
                };

                // Convert to Azure Table Entity
                var entity = new TableEntity("PRODUCT", product.Id)
                {
                    { "Name", product.Name },
                    { "Description", product.Description ?? "" },
                    { "Price", Convert.ToDouble(product.Price) },
                    { "ImageUrl", product.ImageUrl ?? "" }
                };

                // Save to Table Storage
                await table.AddEntityAsync(opts.ProductsTable, entity);
            }
        }
    }
}
