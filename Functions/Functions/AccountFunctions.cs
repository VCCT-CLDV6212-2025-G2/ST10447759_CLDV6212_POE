using AzureRetailHub.Functions.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;

namespace AzureRetailHub.Functions.Functions
{
    public class AccountFunctions
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<AccountFunctions> _logger;

        public AccountFunctions(ApplicationDbContext context, ILogger<AccountFunctions> logger)
        {
            _context = context;
            _logger = logger;
        }

        public record RegisterModel(string Email, string Password, string FullName, string Phone);

        [Function("RegisterUser")]
        public async Task<IActionResult> Register(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "account/register")] HttpRequest req)
        {
            _logger.LogInformation("C# HTTP trigger function processing user registration.");
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            var model = JsonSerializer.Deserialize<RegisterModel>(requestBody, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            if (model == null || string.IsNullOrEmpty(model.Email) || string.IsNullOrEmpty(model.Password))
            {
                return new BadRequestObjectResult("Email and password are required.");
            }

            var existingUser = await _context.Users.FirstOrDefaultAsync(u => u.Email == model.Email);
            if (existingUser != null)
            {
                return new ConflictObjectResult("User with this email already exists.");
            }

            var user = new User
            {
                Email = model.Email,
                FullName = model.FullName,
                Phone = model.Phone,
                IsAdmin = false,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(model.Password)
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            // --- THIS IS THE FIX ---
            // Create a DTO to safely return, breaking the cycle
            var userDto = new UserDto
            {
                Id = user.Id,
                Email = user.Email,
                FullName = user.FullName,
                Phone = user.Phone,
                IsAdmin = user.IsAdmin
            };

            return new OkObjectResult(userDto);
        }

        public record LoginModel(string Email, string Password);

        [Function("LoginUser")]
        public async Task<IActionResult> Login(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "account/login")] HttpRequest req)
        {
            _logger.LogInformation("C# HTTP trigger function processing user login.");
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            var model = JsonSerializer.Deserialize<LoginModel>(requestBody, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            if (model == null || string.IsNullOrEmpty(model.Email) || string.IsNullOrEmpty(model.Password))
            {
                return new BadRequestObjectResult("Email and password are required.");
            }

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == model.Email);
            if (user == null)
            {
                return new UnauthorizedObjectResult("Invalid email or password.");
            }

            if (!BCrypt.Net.BCrypt.Verify(model.Password, user.PasswordHash))
            {
                return new UnauthorizedObjectResult("Invalid email or password.");
            }

            // --- THIS IS THE FIX ---
            // Create a DTO to safely return
            var userDto = new UserDto
            {
                Id = user.Id,
                Email = user.Email,
                FullName = user.FullName,
                Phone = user.Phone,
                IsAdmin = user.IsAdmin
            };

            return new OkObjectResult(userDto);
        }

        [Function("GetUserById")]
        public async Task<IActionResult> GetUserById(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "users/{id}")] HttpRequest req,
            string id)
        {
            _logger.LogInformation("C# HTTP trigger function processed a request to get user by id.");

            // --- THIS IS THE FIX ---
            var user = await _context.Users
                .Select(u => new UserDto
                {
                    Id = u.Id,
                    Email = u.Email,
                    FullName = u.FullName,
                    Phone = u.Phone,
                    IsAdmin = u.IsAdmin
                })
                .FirstOrDefaultAsync(u => u.Id == id);

            if (user == null)
            {
                return new NotFoundResult();
            }
            return new OkObjectResult(user);
        }
    }
}