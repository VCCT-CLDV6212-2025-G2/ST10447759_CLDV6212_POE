using AzureRetailHub.Models;
using AzureRetailHub.Services;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using System.Threading.Tasks;

namespace AzureRetailHub.Controllers
{
    public class OrdersController : Controller
    {
        private readonly FunctionApiClient _api;

        public OrdersController(FunctionApiClient api)
        {
            _api = api;
        }

        // Helper to get current user from Session
        private UserViewModel? GetCurrentUser()
        {
            var userJson = HttpContext.Session.GetString("LoggedInUser");
            if (string.IsNullOrEmpty(userJson))
            {
                return null;
            }
            return JsonSerializer.Deserialize<UserViewModel>(userJson);
        }

        // GET: Orders
        public async Task<IActionResult> Index()
        {
            var user = GetCurrentUser();
            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            if (user.IsAdmin)
            {
                // Admin gets all orders
                var allOrders = await _api.GetAllOrdersAsync();
                return View(allOrders);
            }
            else
            {
                // Customer gets just their orders
                var userOrders = await _api.GetOrdersByUserAsync(user.Id);
                return View(userOrders);
            }
        }

        // GET: Orders/Details/5
        public async Task<IActionResult> Details(string id)
        {
            var user = GetCurrentUser();
            if (user == null) return RedirectToAction("Login", "Account");

            // We'll just get all orders for now and find the one.
            // A better solution would be a "GetOrderById" function.
            var orders = user.IsAdmin ? await _api.GetAllOrdersAsync() : await _api.GetOrdersByUserAsync(user.Id);
            var order = orders.FirstOrDefault(o => o.Id == id);

            if (order == null) return NotFound();

            return View(order);
        }

        // GET: Orders/Edit/5
        public async Task<IActionResult> Edit(string id)
        {
            var user = GetCurrentUser();
            if (user == null || !user.IsAdmin) return RedirectToAction("Index", "Home");

            var orders = await _api.GetAllOrdersAsync();
            var order = orders.FirstOrDefault(o => o.Id == id);

            if (order == null) return NotFound();
            return View(order);
        }

        // POST: Orders/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, [FromForm] string status)
        {
            var user = GetCurrentUser();
            if (user == null || !user.IsAdmin) return RedirectToAction("Index", "Home");

            var success = await _api.UpdateOrderStatusAsync(id, status);
            if (!success)
            {
                ModelState.AddModelError("", "Failed to update order status.");
                // Refetch order data for the view
                var orders = await _api.GetAllOrdersAsync();
                var order = orders.FirstOrDefault(o => o.Id == id);
                return View(order);
            }

            return RedirectToAction(nameof(Index));
        }
    }
}