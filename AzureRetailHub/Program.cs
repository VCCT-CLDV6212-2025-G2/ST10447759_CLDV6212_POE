using AzureRetailHub.Services;
using AzureRetailHub.Settings;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// --- NEW, SIMPLIFIED REGISTRATIONS ---

// 1. Configure FunctionApi settings
builder.Services.Configure<FunctionApiOptions>(builder.Configuration.GetSection("FunctionApi"));

// 2. Register the FunctionApiClient (our ONLY link to the backend)
builder.Services.AddHttpClient<FunctionApiClient>();

// 3. Register Session and Cart
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<Cart>(sp => Cart.GetCart(sp));

// --- END NEW REGISTRATIONS ---

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
}
app.UseStaticFiles();
app.UseRouting();

app.UseSession(); // <-- IMPORTANT: Enable Session

// NO app.UseAuthentication() or app.UseAuthorization() needed
// We will manage this manually in the controllers

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// NO app.MapRazorPages() needed

// NO SEEDING SCRIPT NEEDED

app.Run();