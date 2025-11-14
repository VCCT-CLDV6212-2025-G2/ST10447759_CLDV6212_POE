/*
 * Jeron Okkers
 * ST10447759
 * CLDV6212
 * MODIFIED FOR PART 3
 * Reads/Updates Orders from SQL Database.
 * Create is now handled by CartController.
 */

using System;
using System.Linq;
using System.Threading.Tasks;
using AzureRetailHub.Data;           // ApplicationDbContext
using AzureRetailHub.Models;         // Order, OrderDetailViewModel, Product, CustomerDto, OrderDto
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;

namespace AzureRetailHub.Controllers
{
    [Authorize] // All order actions require login
    public class OrdersController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public OrdersController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: Orders
        public async Task<IActionResult> Index()
        {
            IQueryable<Order> ordersQuery; // Uses the 'Order' entity

            if (User.IsInRole("Admin"))
            {
                // Admins see all orders
                ordersQuery = _context.Orders.Include(o => o.ApplicationUser);
            }
            else
            {
                // Customers see only their own orders
                var userId = _userManager.GetUserId(User);
                ordersQuery = _context.Orders
                    .Include(o => o.ApplicationUser)
                    .Where(o => o.ApplicationUserId == userId);
            }

            var orders = await ordersQuery
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();

            return View(orders); // Pass the List<Order> entities to the view
        }

        // GET: Orders/Details/{id}
        public async Task<IActionResult> Details(string id)
        {
            if (string.IsNullOrEmpty(id)) return NotFound();

            var order = await _context.Orders
                .Include(o => o.ApplicationUser) // Load customer info
                .Include(o => o.Items)           // Load order items
                .ThenInclude(i => i.Product)     // Load product info for each item
                .FirstOrDefaultAsync(o => o.Id == id);

            if (order == null) return NotFound();

            // Security check: Only Admins or owners can view
            var currentUserId = _userManager.GetUserId(User);
            if (!User.IsInRole("Admin") && order.ApplicationUserId != currentUserId)
            {
                return Forbid();
            }

            // --- FIX IS HERE ---
            // Build view model
            var viewModel = new OrderDetailViewModel
            {
                // Map the Order ENTITY to the Order DTO
                Order = new OrderDto
                {
                    Id = order.Id,
                    CustomerId = order.ApplicationUserId, // Corrected property name
                    OrderDate = order.OrderDate,
                    Status = order.Status
                },
                Customer = new CustomerDto
                {
                    RowKey = order.ApplicationUser?.Id ?? "",
                    FullName = order.ApplicationUser?.FullName ?? "N/A",
                    Email = order.ApplicationUser?.Email
                },
                // Map the OrderItem ENTITY to the view model's ItemDetail DTO
                Items = order.Items.Select(item => new OrderDetailViewModel.ItemDetail
                {
                    ProductId = item.ProductId,
                    ProductName = item.Product?.Name ?? "Product not found",
                    Quantity = item.Quantity,
                    Price = item.Price
                }).ToList()
            };

            return View(viewModel);
        }

        // GET: Orders/Edit/{id}
        // GET: Orders/Edit/{id}
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(string id)
        {
            if (string.IsNullOrEmpty(id)) return NotFound();

            // Use Include() to also get the related ApplicationUser (customer)
            var order = await _context.Orders
                .Include(o => o.ApplicationUser)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (order == null) return NotFound();

            return View(order);
        }

        // POST: Orders/Edit/{id}
        // POST: Orders/Edit/{id}
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(string id, [FromForm] string Status) // <-- This is the corrected line
        {
            var orderToUpdate = await _context.Orders.FindAsync(id);
            if (orderToUpdate == null)
            {
                return NotFound();
            }

            // Update only the status from the form
            orderToUpdate.Status = Status;

            try
            {
                // Save the changes (no ModelState check needed)
                _context.Update(orderToUpdate);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.Orders.Any(e => e.Id == orderToUpdate.Id))
                    return NotFound();
                else
                    throw;
            }

            // Go back to the list
            return RedirectToAction(nameof(Index));
        }
    }
}