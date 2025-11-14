/*
 * FILE: ProductsController.cs
 * MODIFIED FOR PART 3
 * This controller now uses ApplicationDbContext (SQL) for metadata.
 * It still uses FunctionApiClient (from Part 2) for image uploads.
 */
using System;
using System.Linq;
using System.Threading.Tasks;
using AzureRetailHub.Data;      // ApplicationDbContext
using AzureRetailHub.Models;     // Product
using AzureRetailHub.Services;   // FunctionApiClient
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization; // For admin security

namespace AzureRetailHub.Controllers
{
    public class ProductsController : Controller
    {
        private readonly ApplicationDbContext _context; // <-- CHANGED
        private readonly FunctionApiClient _fx;

        public ProductsController(
            ApplicationDbContext context,    // <-- CHANGED
            FunctionApiClient fx)
        {
            _context = context;
            _fx = fx;
        }

        // GET: Products (with simple search on Name)
        // This now reads from SQL
        public async Task<IActionResult> Index(string? q)
        {
            var query = _context.Products.AsQueryable();

            if (!string.IsNullOrWhiteSpace(q))
            {
                query = query.Where(p => p.Name.Contains(q));
            }

            var list = await query.ToListAsync();
            ViewBag.Query = q;
            return View(list);
        }

        // GET: Products/Details/{id}
        // This now reads from SQL
        public async Task<IActionResult> Details(string id)
        {
            if (string.IsNullOrWhiteSpace(id)) return NotFound();
            var product = await _context.Products.FindAsync(id);
            if (product == null) return NotFound();
            return View(product);
        }

        // GET: Products/Create
        [Authorize(Roles = "Admin")] // Only Admins can create
        public IActionResult Create() => View(new Product());

        // POST: Products/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create(Product model, IFormFile? imageFile)
        {
            if (!ModelState.IsValid) return View(model);

            model.Id = Guid.NewGuid().ToString("N");

            // --- THIS LOGIC IS FROM PART 2 (UNCHANGED) ---
            string? imageUrl = model.ImageUrl;
            if (imageFile is not null && imageFile.Length > 0)
            {
                imageUrl = await _fx.PostFileAsync("products/image", imageFile);
                if (imageUrl is null)
                {
                    ModelState.AddModelError("", "Image upload failed via Functions API.");
                    return View(model);
                }
                model.ImageUrl = imageUrl; // Set the URL on the model
            }
            // --- END PART 2 LOGIC ---

            // Save metadata to SQL database
            _context.Products.Add(model);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // GET: Products/Edit/{id}
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(string id)
        {
            if (string.IsNullOrWhiteSpace(id)) return NotFound();
            var product = await _context.Products.FindAsync(id);
            if (product == null) return NotFound();
            return View(product);
        }

        // POST: Products/Edit/{id}
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(string id, Product model, IFormFile? imageFile)
        {
            if (id != model.Id) return BadRequest();
            if (!ModelState.IsValid) return View(model);

            // --- THIS LOGIC IS FROM PART 2 (UNCHANGED) ---
            string? imageUrl = model.ImageUrl;
            if (imageFile is not null && imageFile.Length > 0)
            {
                imageUrl = await _fx.PostFileAsync("products/image", imageFile);
                if (imageUrl is null)
                {
                    ModelState.AddModelError("", "Image upload failed via Functions API.");
                    return View(model);
                }
                model.ImageUrl = imageUrl; // Set the new URL
            }
            // --- END PART 2 LOGIC ---

            try
            {
                // Update metadata in SQL database
                _context.Update(model);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.Products.Any(e => e.Id == model.Id))
                    return NotFound();
                else
                    throw;
            }
            return RedirectToAction(nameof(Index));
        }

        // GET: Products/Delete/{id}
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(string id)
        {
            if (string.IsNullOrWhiteSpace(id)) return NotFound();
            var product = await _context.Products.FindAsync(id);
            if (product == null) return NotFound();
            return View(product);
        }

        // POST: Products/Delete/{id}
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product != null)
            {
                _context.Products.Remove(product);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }
    }
}