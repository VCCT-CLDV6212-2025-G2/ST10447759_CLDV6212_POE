using AzureRetailHub.Models;
using AzureRetailHub.Services;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace AzureRetailHub.Controllers
{
    public class CartController : Controller
    {
        private readonly Cart _cart;
        private readonly FunctionApiClient _api;

        public CartController(Cart cart, FunctionApiClient api)
        {
            _cart = cart;
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

        // GET: /Cart/
        public async Task<IActionResult> Index()
        {
            // We must re-fetch product data to ensure prices are up to date
            // This also protects against "stale" products in the cart
            var products = await _api.GetProductsAsync();
            _cart.ValidateCart(products);
            return View(_cart);
        }

        // POST: /Cart/Add
        [HttpPost]
        public async Task<IActionResult> Add(string id, int quantity = 1)
        {
            if (string.IsNullOrEmpty(id)) return NotFound();

            var product = await _api.GetProductByIdAsync(id);
            if (product == null) return NotFound();

            _cart.AddItem(product, quantity);

            return RedirectToAction(nameof(Index));
        }

        // POST: /Cart/Remove
        [HttpPost]
        public IActionResult Remove(string id)
        {
            _cart.RemoveItem(id);
            return RedirectToAction(nameof(Index));
        }

        // GET: /Cart/Checkout
        public IActionResult Checkout()
        {
            if (GetCurrentUser() == null)
            {
                return RedirectToAction("Login", "Account", new { returnUrl = "/Cart/Checkout" });
            }

            if (_cart.Items.Count == 0)
            {
                return RedirectToAction("Index", "Products");
            }
            return View();
        }

        // POST: /Cart/Checkout
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Checkout(object model) // We don't need to bind anything
        {
            var user = GetCurrentUser();
            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            if (_cart.Items.Count == 0)
            {
                return RedirectToAction(nameof(Index));
            }

            // 1. Create the message DTO for the function
            var orderMessage = new FunctionApiClient.OrderQueueMessage(
                UserId: user.Id,
                Status: "Processing",
                Items: _cart.Items.Select(item => new FunctionApiClient.OrderItemMessage(
                    item.ProductId,
                    item.Quantity,
                    item.Price
                )).ToList()
            );

            // 2. Call the function to enqueue the order
            var success = await _api.CreateOrderAsync(orderMessage);

            if (!success)
            {
                ModelState.AddModelError("", "There was an error placing your order. Please try again.");
                return View();
            }

            // 3. Clear the cart
            _cart.Clear();

            // 4. Redirect to Order History
            return RedirectToAction("Index", "Orders");
        }
    }
}