using Inventory.ViewModels;
using Inventory.Web.ViewModels.Home;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Text.Json;

namespace Inventory.Web.Controllers
{
    public class HomeController : Controller
    {
        private readonly HttpClient _client;
        private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

        public HomeController(IHttpClientFactory httpClientFactory)
        {
            _client = httpClientFactory.CreateClient("InventoryAPI");
        }

        [HttpGet]
        [Authorize(Roles = "SuperAdmin, Admin , Super Admin")]
        public async Task<IActionResult> Index()
        {
            var vm = new DashboardVM();

            try
            {
                var productsTask = _client.GetFromJsonAsync<List<RecentProductVM>>("Products", JsonOptions);
                var categoriesTask = _client.GetFromJsonAsync<List<object>>("Categories", JsonOptions);
                var usersTask = _client.GetFromJsonAsync<List<object>>("Accounts/users", JsonOptions);

                await Task.WhenAll(productsTask, categoriesTask, usersTask);

                var products = await productsTask ?? new();
                var categories = await categoriesTask ?? new();
                var users = await usersTask ?? new();

                vm.TotalProducts = products.Count;
                vm.LowStockCount = products.Count(p => p.StockQuantity <= 5);
                vm.TotalCategories = categories.Count;
                vm.TotalUsers = users.Count;
                vm.RecentProducts = products.OrderByDescending(p => p.Id).Take(5).ToList();
            }
            catch
            {
            }

            return View(vm);
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
