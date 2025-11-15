using AzureRetailHub.Functions.Data;
using AzureRetailHub.Functions.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace AzureRetailHub.Functions.Functions
{

    public class ProductsFunctions
    {
        private readonly ApplicationDbContext _context;
        private readonly BlobStorageService _blob;
        private readonly ILogger<ProductsFunctions> _logger;

        public ProductsFunctions(ApplicationDbContext context, BlobStorageService blob, ILogger<ProductsFunctions> logger)
        {
            _context = context;
            _blob = blob;
            _logger = logger;
        }

        // --- GET Products ---
        [Function("GetProducts")]
        public async Task<IActionResult> GetProducts(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "products")] HttpRequest req)
        {
            _logger.LogInformation("C# HTTP trigger function processed a request to get all products.");

            var products = await _context.Products
                .Select(p => new ProductDto
                {
                    Id = p.Id,
                    Name = p.Name,
                    Description = p.Description,
                    Price = p.Price,
                    ImageUrl = p.ImageUrl
                })
                .ToListAsync();

            return new OkObjectResult(products);
        }

        // --- GET Product By Id ---
        [Function("GetProductById")]
        public async Task<IActionResult> GetProductById(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "products/{id}")] HttpRequest req,
            string id)
        {
            _logger.LogInformation("C# HTTP trigger function processed a request to get product by id.");

            var product = await _context.Products
                .Select(p => new ProductDto
                {
                    Id = p.Id,
                    Name = p.Name,
                    Description = p.Description,
                    Price = p.Price,
                    ImageUrl = p.ImageUrl
                })
                .FirstOrDefaultAsync(p => p.Id == id);

            return product == null ? new NotFoundResult() : new OkObjectResult(product);
        }

        // --- CREATE Product (FIXED) ---
        [Function("CreateProduct")]
        public async Task<IActionResult> CreateProduct(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "products")] HttpRequest req)
        {
            _logger.LogInformation("C# HTTP trigger function processed a request to create a product.");
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            var product = JsonSerializer.Deserialize<Product>(requestBody, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            if (product == null)
            {
                return new BadRequestObjectResult("Invalid product data.");
            }

            // --- THIS IS THE FIX ---
            // We must assign the ID here, in the function.
            product.Id = System.Guid.NewGuid().ToString("N");
            // --- END FIX ---

            _context.Products.Add(product);
            await _context.SaveChangesAsync();

            // --- THIS IS THE SECOND FIX ---
            // Return a DTO to prevent circular reference crash
            var productDto = new ProductDto
            {
                Id = product.Id,
                Name = product.Name,
                Description = product.Description,
                Price = product.Price,
                ImageUrl = product.ImageUrl
            };

            return new OkObjectResult(productDto);
        }

        // --- UPDATE Product (FIXED) ---
        [Function("UpdateProduct")]
        public async Task<IActionResult> UpdateProduct(
            [HttpTrigger(AuthorizationLevel.Anonymous, "put", Route = "products/{id}")] HttpRequest req,
            string id)
        {
            _logger.LogInformation("C# HTTP trigger function processed a request to update a product.");
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            var productUpdate = JsonSerializer.Deserialize<Product>(requestBody, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            if (productUpdate == null || id != productUpdate.Id)
            {
                return new BadRequestObjectResult("Invalid product data.");
            }

            var product = await _context.Products.FindAsync(id);
            if (product == null)
            {
                return new NotFoundResult();
            }

            // Update properties
            product.Name = productUpdate.Name;
            product.Description = productUpdate.Description;
            product.Price = productUpdate.Price;
            product.ImageUrl = productUpdate.ImageUrl;

            _context.Products.Update(product);
            await _context.SaveChangesAsync();

            // --- THIS IS THE FIX ---
            // Return a DTO to prevent circular reference crash
            var productDto = new ProductDto
            {
                Id = product.Id,
                Name = product.Name,
                Description = product.Description,
                Price = product.Price,
                ImageUrl = product.ImageUrl
            };

            return new OkObjectResult(productDto);
        }

        // --- DELETE Product ---
        [Function("DeleteProduct")]
        public async Task<IActionResult> DeleteProduct(
            [HttpTrigger(AuthorizationLevel.Anonymous, "delete", Route = "products/{id}")] HttpRequest req,
            string id)
        {
            _logger.LogInformation("C# HTTP trigger function processed a request to delete a product.");
            var product = await _context.Products.FindAsync(id);
            if (product == null)
            {
                return new NotFoundResult();
            }

            _context.Products.Remove(product);
            await _context.SaveChangesAsync();

            return new OkResult();
        }

        // --- UPLOAD Image ---
        [Function("UploadImage")]
        public async Task<IActionResult> UploadImage(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "products/image")] HttpRequest req)
        {
            _logger.LogInformation("C# HTTP trigger function processed a request to upload an image.");

            if (req.Body == null || req.ContentLength == 0)
            {
                return new BadRequestObjectResult("No file received.");
            }

            await using var ms = new MemoryStream();
            await req.Body.CopyToAsync(ms);
            ms.Position = 0;

            var contentType = req.ContentType ?? "application/octet-stream";
            var fileName = req.Headers["X-File-Name"].FirstOrDefault() ?? "image.bin";
            var extension = Path.GetExtension(fileName);
            var uniqueFileName = $"{Path.GetFileNameWithoutExtension(Path.GetRandomFileName())}{extension}";

            var fileUrl = await _blob.UploadFileAsync(ms, uniqueFileName, contentType);

            return new OkObjectResult(new { url = fileUrl });
        }
    }
}