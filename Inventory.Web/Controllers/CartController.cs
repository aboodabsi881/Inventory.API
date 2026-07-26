using Inventory.Web.Resources;
using Inventory.Web.ViewModels.Carts;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using System.Net.Http.Json;
using System.Text.Json;

namespace Inventory.Web.Controllers
{
    public class CartController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IStringLocalizer<SharedResource> _localizer;

        public CartController(
            IHttpClientFactory httpClientFactory,
            IStringLocalizer<SharedResource> localizer)
        {
            _httpClientFactory = httpClientFactory;
            _localizer = localizer;
        }

        // GET: Cart
        public async Task<IActionResult> Index()
        {
            var client = _httpClientFactory.CreateClient("InventoryAPI");
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

            var cartItems = await client.GetFromJsonAsync<List<CartVM>>("Carts", options) ?? new List<CartVM>();
            decimal grandTotal = await GetSafeCartTotalAsync(client, options);

            ViewBag.GrandTotal = grandTotal;
            return View(cartItems);
        }

        // POST: Cart/AddOrUpdate
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddOrUpdate(int productId, int change = 1)
        {
            if (productId <= 0)
            {
                return BadRequest(new { icon = "error", message = "Invalid Product ID." });
            }

            var client = _httpClientFactory.CreateClient("InventoryAPI");

            // 💡 Calls the EXACT backend endpoint: POST api/carts/items?productId=5&change=1
            var response = await client.PostAsync($"Carts/items?productId={productId}&change={change}", null);

            if (response.IsSuccessStatusCode)
            {
                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                var responseContent = await response.Content.ReadAsStringAsync();

                // Check if API returned null / item removed
                CartVM? updatedItem = null;
                if (!responseContent.Contains("removed from cart"))
                {
                    updatedItem = JsonSerializer.Deserialize<CartVM>(responseContent, options);
                }

                decimal grandTotal = await GetSafeCartTotalAsync(client, options);

                return Ok(new
                {
                    icon = "success",
                    message = "Cart updated successfully.",
                    item = updatedItem,
                    grandTotal = grandTotal,
                    removed = updatedItem == null
                });
            }

            var errorDetails = await response.Content.ReadAsStringAsync();
            return BadRequest(new { icon = "error", message = $"API Error: {errorDetails}" });
        }

        // POST: Cart/Remove/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Remove(int id)
        {
            if (id <= 0)
            {
                return BadRequest(new { icon = "error", message = "Invalid Cart ID." });
            }

            var client = _httpClientFactory.CreateClient("InventoryAPI");

            // 💡 Matches API route: DELETE api/carts/{cartId}
            var response = await client.DeleteAsync($"Carts/{id}");

            if (response.IsSuccessStatusCode)
            {
                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                decimal grandTotal = await GetSafeCartTotalAsync(client, options);

                return Ok(new
                {
                    icon = "info",
                    message = "Item removed from cart.",
                    cartId = id,
                    grandTotal = grandTotal
                });
            }

            return BadRequest(new { icon = "error", message = "Failed to remove item." });
        }

        private async Task<decimal> GetSafeCartTotalAsync(HttpClient client, JsonSerializerOptions options)
        {
            try
            {
                var response = await client.GetAsync("Carts/total");
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
                // Fallback
            }

            return 0;
        }
    }
}