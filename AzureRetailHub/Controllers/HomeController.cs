/*
 * MODIFIED FOR PART 3
 * This controller now reads Products from the SQL Database (ApplicationDbContext)
 * It also includes session test methods.
 */
using AzureRetailHub.Models;
using AzureRetailHub.Services;
using AzureRetailHub.Data;          // <-- NEW: For SQL Database
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore; // <-- NEW: For SQL Database
using Microsoft.AspNetCore.Http;     // <-- NEW: For Session Test
using System.Diagnostics;          // <-- NEW: For ErrorViewModel

namespace AzureRetailHub.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context; // <-- CHANGED: Use SQL DbContext

        // CHANGED: Inject ApplicationDbContext
        public HomeController(ApplicationDbContext context)
        {
            _context = context;
        }

        // CHANGED: Load products from SQL Database
        public async Task<IActionResult> Index()
        {
            var products = await _context.Products.ToListAsync();

            var viewModel = new HomeViewModel
            {
                NewArrivals = products.OrderByDescending(p => p.Id).Take(4).ToList(), // Just an example
                BestSellers = products.Take(4).ToList()
            };

            return View(viewModel);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        // ==========================================================
        // SESSION TEST CODE
        // ==========================================================
        public IActionResult TestSessionWrite()
        {
            // Test 1: Save a simple string
            HttpContext.Session.SetString("TestMessage", "Hello, the session is working!");

            // Test 2: Save a complex object (this tests your SessionExtensions.cs)
            var testObj = new { Name = "Test", Value = 123 };
            HttpContext.Session.Set("TestObject", testObj);

            // Now redirect to the read page
            return RedirectToAction("TestSessionRead");
        }

        public IActionResult TestSessionRead()
        {
            // Read Test 1
            ViewBag.TestMessage = HttpContext.Session.GetString("TestMessage");

            // Read Test 2
            var testObj = HttpContext.Session.Get<dynamic>("TestObject");
            ViewBag.TestObject = testObj?.ToString() ?? "Test Object Not Found";

            return View();
        }
    }

    // This ViewModel class can stay here
    public class HomeViewModel
    {
        public List<Product> NewArrivals { get; set; } = new();
        public List<Product> BestSellers { get; set; } = new();
    }
}