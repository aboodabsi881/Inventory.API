using Inventory.Web.Resources;
using Inventory.Web.ViewModels.Favorites;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using System.Net.Http.Json;
using System.Text.Json;

namespace Inventory.Web.Controllers
{
    public class FavoriteController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IStringLocalizer<SharedResource> _localizer;

        public FavoriteController(
            IHttpClientFactory httpClientFactory,
            IStringLocalizer<SharedResource> localizer)
        {
            _httpClientFactory = httpClientFactory;
            _localizer = localizer;
        }

        // GET: Favorite
        public async Task<IActionResult> Index()
        {
            var client = _httpClientFactory.CreateClient("InventoryAPI");
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

            var favorites = await client.GetFromJsonAsync<List<FavoriteVM>>("Favorites", options);

            return View(favorites ?? new List<FavoriteVM>());
        }

        // POST: Favorite/Add
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Add(int productId)
        {
            if (productId <= 0)
            {
                return BadRequest(new { icon = "error", message = _localizer["ProductNotFound"].Value });
            }

            var client = _httpClientFactory.CreateClient("InventoryAPI");

            // 💡 Matches API route: POST api/favorites/toggle/{productId}
            var response = await client.PostAsync($"Favorites/toggle/{productId}", null);

            if (response.IsSuccessStatusCode)
            {
                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                var result = await response.Content.ReadFromJsonAsync<FavoriteVM>(options);

                return Ok(new
                {
                    icon = "success",
                    message = result?.IsFavorite == true
                        ? _localizer["AddedToFavorite"].Value
                        : _localizer["RemovedFromFavorite"].Value,
                    isFavorite = result?.IsFavorite ?? false,
                    productId = productId
                });
            }

            var errorDetails = await response.Content.ReadAsStringAsync();
            return BadRequest(new { icon = "error", message = $"API Error: {errorDetails}" });
        }

        // POST: Favorite/DeleteConfirmed/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (id <= 0)
            {
                return BadRequest(new { icon = "error", message = _localizer["ProductNotFound"].Value });
            }

            var client = _httpClientFactory.CreateClient("InventoryAPI");

            // 💡 Matches API route: DELETE api/favorites/{id}
            var response = await client.DeleteAsync($"Favorites/{id}");

            if (response.IsSuccessStatusCode)
            {
                return Ok(new
                {
                    icon = "info",
                    message = _localizer["RemovedFromFavorite"].Value,
                    deletedId = id,
                    isFavorite = false
                });
            }

            return BadRequest(new { icon = "error", message = _localizer["ProductNotFound"].Value });
        }
    }
}