using Inventory.Web.Resources;
using Inventory.Web.ViewModels.Carts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using System.Text.Json;

namespace Inventory.Web.Controllers
{
    [Authorize]
    public class CartController : Controller
    {
        private readonly HttpClient _client;
        private readonly IStringLocalizer<SharedResource> _localizer;
        private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

        public CartController(IHttpClientFactory httpClientFactory, IStringLocalizer<SharedResource> localizer)
        {
            _client = httpClientFactory.CreateClient("InventoryAPI");
            _localizer = localizer;
        }

        public async Task<IActionResult> Index()
        {
            List<CartVM> cartItems = new();
            decimal grandTotal = 0;

            try
            {
                cartItems = await _client.GetFromJsonAsync<List<CartVM>>("Carts", JsonOptions) ?? new List<CartVM>();
                grandTotal = await GetSafeCartTotalAsync();
            }
            catch
            {
            }

            ViewBag.GrandTotal = grandTotal;
            return View(cartItems);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddOrUpdate(int productId, int change = 1)
        {
            if (productId <= 0)
                return BadRequest(new { icon = "error", message = "Invalid Product ID." });

            var response = await _client.PostAsync($"Carts/items?productId={productId}&change={change}", null);
            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                CartVM? updatedItem = null;

                if (!responseContent.Contains("removed from cart"))
                {
                    updatedItem = JsonSerializer.Deserialize<CartVM>(responseContent, JsonOptions);
                }

                decimal grandTotal = await GetSafeCartTotalAsync();

                return Ok(new
                {
                    icon = "success",
                    message = "Cart updated successfully.",
                    item = updatedItem,
                    grandTotal,
                    removed = updatedItem == null
                });
            }

            var errorDetails = await response.Content.ReadAsStringAsync();
            return BadRequest(new { icon = "error", message = $"API Error: {errorDetails}" });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Remove(int id)
        {
            if (id <= 0)
                return BadRequest(new { icon = "error", message = "Invalid Cart ID." });

            var response = await _client.DeleteAsync($"Carts/{id}");
            if (response.IsSuccessStatusCode)
            {
                decimal grandTotal = await GetSafeCartTotalAsync();

                return Ok(new
                {
                    icon = "info",
                    message = "Item removed from cart.",
                    cartId = id,
                    grandTotal
                });
            }

            return BadRequest(new { icon = "error", message = "Failed to remove item." });
        }

        private async Task<decimal> GetSafeCartTotalAsync()
        {
            try
            {
                var response = await _client.GetAsync("Carts/total");
                if (!response.IsSuccessStatusCode) return 0;

                var content = await response.Content.ReadAsStringAsync();
                if (decimal.TryParse(content, out decimal simpleTotal))
                    return simpleTotal;

                using var doc = JsonDocument.Parse(content);
                var root = doc.RootElement;
                if (root.ValueKind == JsonValueKind.Object && root.TryGetProperty("total", out var totalProp))
                {
                    return totalProp.GetDecimal();
                }
            }
            catch
            {
            }

            return 0;
        }
    }
}