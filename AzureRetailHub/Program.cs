/*
 * ==================================================
 * FINAL, CLEANED-UP Program.cs for Part 3
 * ==================================================
 */

using AzureRetailHub.Services;
using AzureRetailHub.Settings;
using Microsoft.Extensions.Options;
using AzureRetailHub.Data;
using AzureRetailHub.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);
var configuration = builder.Configuration;

// ---------------------------
// DATABASE + IDENTITY SETUP
// ---------------------------

// Get the single connection string
var connectionString = configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

// Add SQL Server DbContext
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));

// Add Identity (Users + Roles)
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    options.SignIn.RequireConfirmedAccount = false;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireDigit = false;
    options.Password.RequiredLength = 6;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders()
.AddDefaultUI();

// ---------------------------
// SESSION (For Cart)
// This is the entire, correct block.
// ---------------------------
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// 1. Add ONE HttpContextAccessor
builder.Services.AddHttpContextAccessor();
// 2. Add ONE Scoped Cart service (with the full namespace)
builder.Services.AddScoped<AzureRetailHub.Services.Cart>(sp => AzureRetailHub.Services.Cart.GetCart(sp));

// ---------------------------
// MVC + AZURE SERVICES
// ---------------------------
builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();

// Bind storage settings
builder.Services.Configure<StorageOptions>(
    builder.Configuration.GetSection("StorageOptions"));

// Register Azure services
builder.Services.AddSingleton<TableStorageService>();
builder.Services.AddSingleton<BlobStorageService>();
builder.Services.AddSingleton<QueueStorageService>();
builder.Services.AddSingleton<FileStorageService>();

// Function service
builder.Services.Configure<FunctionApiOptions>(builder.Configuration.GetSection("FunctionApi"));
builder.Services.AddHttpClient<FunctionApiClient>();

// ---------------------------
// BUILD APP
// ---------------------------

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
}

app.UseStaticFiles();
app.UseRouting();

// This MUST be here, in this order
app.UseSession();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapRazorPages();

// ---------------------------
// SEEDING (Roles + Admin + Products)
// ---------------------------

// This only runs when you debug locally
if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();
    var services = scope.ServiceProvider;

    var context = services.GetRequiredService<ApplicationDbContext>();
    var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
    var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();

    // Ensure DB is created
    await context.Database.MigrateAsync();

    // Seed Identity roles + admin
    await SeedData.SeedIdentityAsync(userManager, roleManager);

    // Seed products (from your old code, this is fine)
    var table = services.GetRequiredService<TableStorageService>();
    var blob = services.GetRequiredService<BlobStorageService>();
    var options = services.GetRequiredService<IOptions<StorageOptions>>().Value;

    await SeedData.SeedProductsAsync(table, blob, options, force: false);
}

app.Run();