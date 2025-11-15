/*
 * FINAL, CORRECTED Program.cs for Functions
 * The bad ApplicationInsights line has been removed.
 */
using AzureRetailHub.Functions.Data;
using AzureRetailHub.Functions.Services;
using AzureRetailHub.Functions.Settings;
using Microsoft.Azure.Functions.Worker;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var host = new HostBuilder()
    .ConfigureFunctionsWebApplication() // This line is correct
    .ConfigureServices((context, services) =>
    {
        // --- Corrected connection string logic ---
        var connectionString = context.Configuration.GetConnectionString("DefaultConnection");
        if (string.IsNullOrEmpty(connectionString))
        {
            connectionString = context.Configuration["Values:ConnectionStrings:DefaultConnection"];
        }

        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlServer(connectionString));

        // --- Bind StorageOptions ---
        services.AddOptions<StorageOptions>()
            .Configure<IConfiguration>((settings, configuration) =>
            {
                var storageSection = configuration.GetSection("StorageOptions");
                if (!storageSection.Exists())
                {
                    storageSection = configuration.GetSection("Values:StorageOptions");
                }
                storageSection.Bind(settings);
            });

        // --- Register Part 1 Storage Services ---
        services.AddSingleton<BlobStorageService>();
        services.AddSingleton<QueueStorageService>();
        services.AddSingleton<FileStorageService>();
    })
    .Build();

host.Run();