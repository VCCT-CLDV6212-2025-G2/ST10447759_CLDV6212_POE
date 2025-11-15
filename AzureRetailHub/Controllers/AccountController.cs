using AzureRetailHub.Models;
using AzureRetailHub.Services;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using System.Threading.Tasks;

namespace AzureRetailHub.Controllers
{
    public class AccountController : Controller
    {
        private readonly FunctionApiClient _api;

        public AccountController(FunctionApiClient api)
        {
            _api = api;
        }

        // GET: /Account/Login
        public IActionResult Login()
        {
            return View();
        }

        // POST: /Account/Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await _api.LoginAsync(model);

            if (user == null)
            {
                ModelState.AddModelError(string.Empty, "Invalid login attempt.");
                return View(model);
            }

            // --- THIS IS THE MANUAL LOGIN ---
            // We serialize the user object and store it in the session
            var userJson = JsonSerializer.Serialize(user);
            HttpContext.Session.SetString("LoggedInUser", userJson);
            // --- END MANUAL LOGIN ---

            return RedirectToAction("Index", "Home");
        }

        // GET: /Account/Register
        public IActionResult Register()
        {
            return View();
        }

        // POST: /Account/Register
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await _api.RegisterAsync(model);

            if (user == null)
            {
                ModelState.AddModelError(string.Empty, "Registration failed. This email may already be in use.");
                return View(model);
            }

            // Automatically log the user in after they register
            var userJson = JsonSerializer.Serialize(user);
            HttpContext.Session.SetString("LoggedInUser", userJson);

            return RedirectToAction("Index", "Home");
        }

        // POST: /Account/Logout
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Logout()
        {
            // --- THIS IS THE MANUAL LOGOUT ---
            HttpContext.Session.Remove("LoggedInUser");
            // --- END MANUAL LOGOUT ---

            return RedirectToAction("Index", "Home");
        }
    }
}