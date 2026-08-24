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
        public async Task<IActionResult> AddOrUpdate(int productId, string? actionType, int change = 1)
        {
            if (productId <= 0)
                return BadRequest(new { icon = "error", message = _localizer["Invalid Product ID."].Value });

            int delta = 1;
            if (actionType == "decrement" || change < 0)
            {
                delta = -1;
            }

            string formattedDelta = delta == -1 ? "-1" : "1";
            var response = await _client.PostAsync($"Carts/items?productId={productId}&change={formattedDelta}", null);

            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                CartVM? updatedItem = null;

                try
                {
                    if (!string.IsNullOrWhiteSpace(responseContent))
                    {
                        updatedItem = JsonSerializer.Deserialize<CartVM>(responseContent, JsonOptions);
                    }
                }
                catch (JsonException)
                {
                    updatedItem = null;
                }

                bool isRemoved = updatedItem == null || updatedItem.Quantity <= 0;
                decimal grandTotal = await GetSafeCartTotalAsync();

                return Ok(new
                {
                    icon = isRemoved ? "info" : "success",
                    message = isRemoved
                        ? _localizer["Item removed from cart."].Value
                        : _localizer["Cart updated successfully."].Value,
                    item = isRemoved ? null : updatedItem,
                    quantity = isRemoved ? 0 : updatedItem!.Quantity,
                    grandTotal,
                    removed = isRemoved
                });
            }

            return BadRequest(new { icon = "error", message = _localizer["Failed to update cart."].Value });
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