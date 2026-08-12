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
                return BadRequest(new { icon = "error", message = _localizer["Invalid Product ID."].Value });

            string formattedChange = change.ToString(System.Globalization.CultureInfo.InvariantCulture);
            var response = await _client.PostAsync($"Carts/items?productId={productId}&change={formattedChange}", null);

            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                CartVM? updatedItem = null;

                try
                {
                    updatedItem = JsonSerializer.Deserialize<CartVM>(responseContent, JsonOptions);
                    if (updatedItem == null || updatedItem.Quantity <= 0)
                    {
                        updatedItem = null;
                    }
                }
                catch (JsonException)
                {
                    updatedItem = null;
                }

                decimal grandTotal = await GetSafeCartTotalAsync();

                return Ok(new
                {
                    icon = "success",
                    message = _localizer["Cart updated successfully."].Value,
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
                return BadRequest(new { icon = "error", message = _localizer["Invalid Cart ID."].Value });

            var response = await _client.DeleteAsync($"Carts/{id}");
            if (response.IsSuccessStatusCode)
            {
                decimal grandTotal = await GetSafeCartTotalAsync();

                return Ok(new
                {
                    icon = "info",
                    message = _localizer["Item removed from cart."].Value,
                    cartId = id,
                    grandTotal
                });
            }

            return BadRequest(new { icon = "error", message = _localizer["Failed to remove item."].Value });
        }

        private async Task<decimal> GetSafeCartTotalAsync()
        {
            try
            {
                var response = await _client.GetAsync("Carts/total");
                if (!response.IsSuccessStatusCode) return 0;

                var content = await response.Content.ReadAsStringAsync();
                if (decimal.TryParse(content, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out decimal simpleTotal))
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