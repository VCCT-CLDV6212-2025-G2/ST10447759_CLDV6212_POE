using AzureRetailHub.Models;
using AzureRetailHub.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.IO;
using System.Threading.Tasks;

namespace AzureRetailHub.Controllers
{
    public class ProductsController : Controller
    {
        private readonly FunctionApiClient _api;

        public ProductsController(FunctionApiClient api)
        {
            _api = api;
        }

        // GET: Products
        public async Task<IActionResult> Index(string? q)
        {
            var products = await _api.GetProductsAsync();
            // --- THIS IS THE FIX ---
            // Filter the products list if a search query 'q' exists
            if (!string.IsNullOrWhiteSpace(q))
            {
                products = products.Where(p =>
                    p.Name.Contains(q, StringComparison.OrdinalIgnoreCase) ||
                    (p.Description != null && p.Description.Contains(q, StringComparison.OrdinalIgnoreCase))
                ).ToList();
            }

            ViewBag.Query = q; // This sends the search term back to the view
            // --- END FIX ---
            // We'll filter in the UI for simplicity, or you can add a search query to your function
            return View(products);
        }

        // GET: Products/Details/5
        public async Task<IActionResult> Details(string id)
        {
            if (id == null) return NotFound();
            var product = await _api.GetProductByIdAsync(id);
            if (product == null) return NotFound();
            return View(product);
        }

        // GET: Products/Create
        public IActionResult Create()
        {
            // We will add an Admin check here later
            return View();
        }

        // POST: Products/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Product product, IFormFile? imageFile)
        {
            if (ModelState.IsValid)
            {
                // 1. Upload the image if it exists
                if (imageFile is not null && imageFile.Length > 0)
                {
                    await using var stream = imageFile.OpenReadStream();
                    var imageUrl = await _api.UploadProductImageAsync(stream, imageFile.FileName);
                    if (imageUrl != null)
                    {
                        product.ImageUrl = imageUrl;
                    }
                }

                // 2. Create the product via the function
                await _api.CreateProductAsync(product);
                return RedirectToAction(nameof(Index));
            }
            return View(product);
        }

        // GET: Products/Edit/5
        public async Task<IActionResult> Edit(string id)
        {
            if (id == null) return NotFound();
            var product = await _api.GetProductByIdAsync(id);
            if (product == null) return NotFound();
            return View(product);
        }

        // POST: Products/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, Product product, IFormFile? imageFile)
        {
            if (id != product.Id) return NotFound();

            if (ModelState.IsValid)
            {
                // 1. Upload a new image if provided
                if (imageFile is not null && imageFile.Length > 0)
                {
                    await using var stream = imageFile.OpenReadStream();
                    var imageUrl = await _api.UploadProductImageAsync(stream, imageFile.FileName);
                    if (imageUrl != null)
                    {
                        product.ImageUrl = imageUrl;
                    }
                }

                // 2. Update the product via the function
                await _api.UpdateProductAsync(product);
                return RedirectToAction(nameof(Index));
            }
            return View(product);
        }

        // GET: Products/Delete/5
        public async Task<IActionResult> Delete(string id)
        {
            if (id == null) return NotFound();
            var product = await _api.GetProductByIdAsync(id);
            if (product == null) return NotFound();
            return View(product);
        }

        // POST: Products/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            await _api.DeleteProductAsync(id);
            return RedirectToAction(nameof(Index));
        }
    }
}