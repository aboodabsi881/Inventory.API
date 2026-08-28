using Inventory.Web.Interfaces;
using Inventory.Web.ViewModels.Home;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;

namespace Inventory.Web.Services
{
    public class ViewHomeService : IViewHomeService
    {
        private readonly HttpClient _client;
        private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

        public ViewHomeService(IHttpClientFactory httpClientFactory)
        {
            _client = httpClientFactory.CreateClient("InventoryAPI");
        }

        public async Task<DashboardVM> GetDashboardMetricsAsync()
        {
            var vm = new DashboardVM();

            // 1. Fetch & Compute Product Metrics
            try
            {
                var products = await _client.GetFromJsonAsync<List<RecentProductVM>>("Products", JsonOptions)
                               ?? new List<RecentProductVM>();

                vm.TotalProducts = products.Count;
                vm.TotalStockUnits = products.Sum(p => p.StockQuantity);
                vm.TotalInventoryValue = products.Sum(p => p.Price * p.StockQuantity);
                vm.LowStockCount = products.Count(p => p.StockQuantity > 0 && p.StockQuantity <= 5);
                vm.OutOfStockCount = products.Count(p => p.StockQuantity <= 0);
                vm.RecentProducts = products.OrderByDescending(p => p.Id).Take(6).ToList();

                // Category Breakdown for Doughnut Chart
                var categoryGroups = products
                    .GroupBy(p => string.IsNullOrWhiteSpace(p.CategoryName) ? "Uncategorized" : p.CategoryName)
                    .OrderByDescending(g => g.Count())
                    .Take(6)
                    .ToList();

                vm.CategoryLabels = categoryGroups.Select(g => g.Key).ToList();
                vm.CategoryProductCounts = categoryGroups.Select(g => g.Count()).ToList();

                // Top Stocked Products for Bar Chart
                var topStocked = products
                    .OrderByDescending(p => p.StockQuantity)
                    .Take(6)
                    .ToList();

                vm.TopProductLabels = topStocked.Select(p => p.Name).ToList();
                vm.TopProductQuantities = topStocked.Select(p => p.StockQuantity).ToList();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Dashboard product metrics error: {ex.Message}");
            }

            // 2. Fetch Category Count
            try
            {
                var categories = await _client.GetFromJsonAsync<List<object>>("Categories", JsonOptions);
                vm.TotalCategories = categories?.Count ?? 0;
            }
            catch { }

            // 3. Fetch System Users Count
            try
            {
                var users = await _client.GetFromJsonAsync<List<object>>("Accounts/users", JsonOptions);
                vm.TotalUsers = users?.Count ?? 0;
            }
            catch { }

            return vm;
        }
    }
}