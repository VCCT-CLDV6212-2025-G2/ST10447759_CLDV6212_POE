using AzureRetailHub.Functions.Data;
using AzureRetailHub.Functions.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System; // <-- Added this
using System.Collections.Generic; // <-- Added this
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace AzureRetailHub.Functions.Functions
{
    // --- HERE ARE THE MISSING CLASSES ---
    // These are the DTOs your web app will send when creating an order
    public class OrderQueueMessage
    {
        public string UserId { get; set; } = "";
        public string Status { get; set; } = "Processing";
        public List<OrderItemMessage> Items { get; set; } = new List<OrderItemMessage>();
    }

    public class OrderItemMessage
    {
        public string ProductId { get; set; } = "";
        public int Quantity { get; set; }
        public decimal Price { get; set; }
    }
    // --- END OF MISSING CLASSES ---

    public class OrdersFunctions
    {
        private readonly ApplicationDbContext _context;
        // The QueueStorageService is no longer needed
        private readonly ILogger<OrdersFunctions> _logger;

        // Constructor is now simpler
        public OrdersFunctions(ApplicationDbContext context, ILogger<OrdersFunctions> logger)
        {
            _context = context;
            _logger = logger;
        }

        [Function("CreateOrder")]
        public async Task<IActionResult> CreateOrder(
         [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "orders")] HttpRequest req)
        {
            _logger.LogInformation("C# HTTP trigger function processed a request to create an order directly.");
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();

            OrderQueueMessage? orderMessage = null;
            try
            {
                // This line will now compile
                orderMessage = JsonSerializer.Deserialize<OrderQueueMessage>(requestBody, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, "Failed to deserialize order message.");
                return new BadRequestObjectResult("Invalid order JSON.");
            }

            if (orderMessage == null || orderMessage.Items.Count == 0)
                return new BadRequestObjectResult("Invalid order data.");

            // Create the new Order entity
            var order = new Order
            {
                Id = Guid.NewGuid().ToString("N"),
                UserId = orderMessage.UserId,
                Status = orderMessage.Status,
                OrderDate = DateTime.UtcNow,
                Items = orderMessage.Items.Select(i => new OrderItem
                {
                    ProductId = i.ProductId,
                    Quantity = i.Quantity,
                    Price = i.Price
                }).ToList()
            };

            try
            {
                _context.Orders.Add(order);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"Successfully saved Order {order.Id} to SQL database.");
                return new OkObjectResult(new { message = "Order placed successfully", orderId = order.Id });
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Failed to save order to database. Likely a Foreign Key violation.");
                return new StatusCodeResult(500); // Internal Server Error
            }
        }

        [Function("GetAllOrders")]
        public async Task<IActionResult> GetAllOrders(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "orders")] HttpRequest req)
        {
            _logger.LogInformation("C# HTTP trigger function processed a request to get all orders.");

            var orders = await _context.Orders
                .Select(o => new OrderDto
                {
                    Id = o.Id,
                    OrderDate = o.OrderDate,
                    Status = o.Status,
                    UserId = o.UserId,
                    User = new UserDto { Id = o.User.Id, Email = o.User.Email, FullName = o.User.FullName },
                    Items = o.Items.Select(i => new OrderItemDto
                    {
                        Id = i.Id,
                        ProductId = i.ProductId,
                        Quantity = i.Quantity,
                        Price = i.Price,
                        Product = new ProductDto { Id = i.Product.Id, Name = i.Product.Name }
                    }).ToList()
                })
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();

            return new OkObjectResult(orders);
        }

        [Function("GetOrdersByUser")]
        public async Task<IActionResult> GetOrdersByUser(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "orders/user/{userId}")] HttpRequest req,
            string userId)
        {
            _logger.LogInformation($"C# HTTP trigger function processed a request to get orders for user {userId}.");

            var orders = await _context.Orders
                .Where(o => o.UserId == userId)
                .Select(o => new OrderDto
                {
                    Id = o.Id,
                    OrderDate = o.OrderDate,
                    Status = o.Status,
                    UserId = o.UserId,
                    User = new UserDto { Id = o.User.Id, Email = o.User.Email, FullName = o.User.FullName },
                    Items = o.Items.Select(i => new OrderItemDto
                    {
                        Id = i.Id,
                        ProductId = i.ProductId,
                        Quantity = i.Quantity,
                        Price = i.Price,
                        Product = new ProductDto { Id = i.Product.Id, Name = i.Product.Name }
                    }).ToList()
                })
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();

            return new OkObjectResult(orders);
        }

        [Function("UpdateOrderStatus")]
        public async Task<IActionResult> UpdateOrderStatus(
            [HttpTrigger(AuthorizationLevel.Anonymous, "put", Route = "orders/{id}/status")] HttpRequest req,
            string id)
        {
            _logger.LogInformation($"C# HTTP trigger function processed a request to update status for order {id}.");
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            var statusUpdate = JsonSerializer.Deserialize<JsonElement>(requestBody);

            if (!statusUpdate.TryGetProperty("status", out var statusElement))
            {
                return new BadRequestObjectResult("Request must include a 'status' field.");
            }

            var newStatus = statusElement.GetString();
            var order = await _context.Orders.FindAsync(id);

            if (order == null)
            {
                return new NotFoundResult();
            }

            order.Status = newStatus;
            _context.Orders.Update(order);
            await _context.SaveChangesAsync();

            // --- THIS IS THE FIX ---
            // Return the updated order as a safe DTO to prevent a circular reference crash
            var orderDto = await _context.Orders
                .Where(o => o.Id == id)
                .Select(o => new OrderDto
                {
                    Id = o.Id,
                    OrderDate = o.OrderDate,
                    Status = o.Status,
                    UserId = o.UserId,
                    User = new UserDto { Id = o.User.Id, Email = o.User.Email, FullName = o.User.FullName },
                    Items = o.Items.Select(i => new OrderItemDto
                    {
                        Id = i.Id,
                        ProductId = i.ProductId,
                        Quantity = i.Quantity,
                        Price = i.Price,
                        Product = new ProductDto { Id = i.Product.Id, Name = i.Product.Name }
                    }).ToList()
                })
                .FirstOrDefaultAsync();
            // --- END FIX ---

            return new OkObjectResult(orderDto);
        }
    }
}