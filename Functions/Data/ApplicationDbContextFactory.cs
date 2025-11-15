using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using System.IO;

namespace AzureRetailHub.Functions.Data
{
    /*
     * This class is specifically for design-time tools like
     * 'Add-Migration' and 'Update-Database'.
     * It tells those tools how to find the connection string
     * in your local.settings.json file.
     */
    public class ApplicationDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
    {
        public ApplicationDbContext CreateDbContext(string[] args)
        {
            // Build a configuration object
            var config = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("local.settings.json", optional: true, reloadOnChange: true)
                .AddEnvironmentVariables()
                .Build();

            // Get the connection string
            var connectionString = config["Values:ConnectionStrings:DefaultConnection"];
            if (string.IsNullOrEmpty(connectionString))
            {
                throw new InvalidOperationException("Could not find connection string 'Values:ConnectionStrings:DefaultConnection' in local.settings.json");
            }
            // Create the DbContextOptions
            var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
            optionsBuilder.UseSqlServer(connectionString);

            // Return the new DbContext
            return new ApplicationDbContext(optionsBuilder.Options);
        }
    }
}