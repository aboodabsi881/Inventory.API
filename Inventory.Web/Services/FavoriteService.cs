using Inventory.Web.Interfaces;
using Inventory.Web.ViewModels.Favorites;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;

namespace Inventory.Web.Services
{
    public class ViewFavoriteService : IViewFavoriteService
    {
        private readonly HttpClient _client;
        private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

        public ViewFavoriteService(IHttpClientFactory httpClientFactory)
        {
            _client = httpClientFactory.CreateClient("InventoryAPI");
        }

        public async Task<List<FavoriteVM>> GetUserFavoritesAsync()
        {
            try
            {
                var response = await _client.GetFromJsonAsync<List<FavoriteVM>>("Favorites", JsonOptions)
                               ?? new List<FavoriteVM>();
                return response.Where(f => f.IsFavorite).ToList();
            }
            catch
            {
                return new List<FavoriteVM>();
            }
        }

        public async Task<(bool Success, bool IsFavorite, string Message)> ToggleFavoriteAsync(int productId)
        {
            var response = await _client.PostAsync($"Favorites/toggle/{productId}", null);
            if (!response.IsSuccessStatusCode)
            {
                var errorDetails = await response.Content.ReadAsStringAsync();
                return (false, false, errorDetails);
            }

            var result = await response.Content.ReadFromJsonAsync<FavoriteVM>(JsonOptions);
            return (true, result?.IsFavorite ?? false, string.Empty);
        }

        public async Task<(bool Success, string Message)> DeleteFavoriteAsync(int id)
        {
            var response = await _client.DeleteAsync($"Favorites/{id}");
            if (!response.IsSuccessStatusCode)
            {
                var errorDetails = await response.Content.ReadAsStringAsync();
                return (false, errorDetails);
            }

            return (true, string.Empty);
        }
    }
}