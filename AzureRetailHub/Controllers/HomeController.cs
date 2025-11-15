using AzureRetailHub.Models;
using AzureRetailHub.Services;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Threading.Tasks;

namespace AzureRetailHub.Controllers
{
    public class HomeController : Controller
    {
        private readonly FunctionApiClient _api;

        public HomeController(FunctionApiClient api)
        {
            _api = api;
        }

        public async Task<IActionResult> Index()
        {
            // Get all products by calling the function
            var products = await _api.GetProductsAsync();

            var viewModel = new HomeViewModel   
            {
                NewArrivals = products.OrderByDescending(p => p.Id).Take(4).ToList(),
                BestSellers = products.Take(4).ToList()
            };

            return View(viewModel);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}