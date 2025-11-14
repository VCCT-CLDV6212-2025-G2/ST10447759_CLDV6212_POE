/*
 * ST10447759
 * CLDV6212
 * Part 3
 */
using AzureRetailHub.Data;
using AzureRetailHub.Models;
using AzureRetailHub.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AzureRetailHub.Controllers
{
    public class CartController : Controller
    {
        private readonly Cart _cart;
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public CartController(Cart cart, ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _cart = cart;
            _context = context;
            _userManager = userManager;
        }

        // GET: /Cart/
        public IActionResult Index()
        {
            return View(_cart);
        }

        // POST: /Cart/Add
        [HttpPost]
        public async Task<IActionResult> Add(string id, int quantity = 1)
        {
            if (string.IsNullOrEmpty(id)) return NotFound();

            var product = await _context.Products.FindAsync(id);
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
        [Authorize] // Must be logged in
        public IActionResult Checkout()
        {
            if (_cart.Items.Count == 0)
            {
                ModelState.AddModelError("", "Your cart is empty!");
                return RedirectToAction("Index", "Products");
            }
            return View();
        }

        // POST: /Cart/Checkout
        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Checkout(Order model) // We can bind to a model for shipping details, etc.
        {
            if (_cart.Items.Count == 0)
            {
                return RedirectToAction(nameof(Index));
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

            // Create the order
            var order = new Order
            {
                ApplicationUserId = user.Id,
                OrderDate = DateTime.UtcNow,
                Status = "Processing" // As per requirement [cite: 35]
            };

            // Add order items from the cart
            foreach (var item in _cart.Items)
            {
                var orderItem = new OrderItem
                {
                    ProductId = item.ProductId,
                    Quantity = item.Quantity,
                    Price = item.Price // Store price at time of purchase
                };
                order.Items.Add(orderItem);
            }

            // Save order to SQL Database
            _context.Orders.Add(order);
            await _context.SaveChangesAsync();

            // Clear the cart
            _cart.Clear();

            // Redirect to a confirmation page (or Order Details)
            return RedirectToAction("Details", "Orders", new { id = order.Id });
        }
    }
}