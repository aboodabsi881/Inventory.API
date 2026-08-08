using Inventory.Web.Resources;
using Inventory.Web.ViewModels.Favorites;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;

namespace Inventory.Web.Controllers
{
    [Authorize]
    public class FavoriteController : Controller
    {
        private readonly HttpClient _client;
        private readonly IStringLocalizer<SharedResource> _localizer;
        private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

        public FavoriteController(
            IHttpClientFactory httpClientFactory,
            IStringLocalizer<SharedResource> localizer)
        {
            _client = httpClientFactory.CreateClient("InventoryAPI");
            _localizer = localizer;
        }

        public async Task<IActionResult> Index()
        {
            List<FavoriteVM> favorites = new();
            try
            {
                var response = await _client.GetFromJsonAsync<List<FavoriteVM>>("Favorites", JsonOptions) ?? new List<FavoriteVM>();
                favorites = response.Where(f => f.IsFavorite).ToList();
            }
            catch
            {
            }

            return View(favorites);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Add(int productId)
        {
            if (productId <= 0)
                return BadRequest(new { icon = "error", message = _localizer["ProductNotFound"].Value });

            var response = await _client.PostAsync($"Favorites/toggle/{productId}", null);
            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<FavoriteVM>(JsonOptions);
                return Ok(new
                {
                    icon = "success",
                    message = result?.IsFavorite == true
                        ? _localizer["AddedToFavorite"].Value
                        : _localizer["RemovedFromFavorite"].Value,
                    isFavorite = result?.IsFavorite ?? false,
                    productId
                });
            }

            var errorDetails = await response.Content.ReadAsStringAsync();
            return BadRequest(new { icon = "error", message = $"API Error: {errorDetails}" });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (id <= 0)
                return BadRequest(new { icon = "error", message = _localizer["ProductNotFound"].Value });

            var response = await _client.DeleteAsync($"Favorites/{id}");
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