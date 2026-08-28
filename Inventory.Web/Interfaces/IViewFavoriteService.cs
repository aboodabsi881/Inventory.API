using Inventory.Web.ViewModels.Favorites;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Inventory.Web.Interfaces
{
    public interface IViewFavoriteService
    {
        Task<List<FavoriteVM>> GetUserFavoritesAsync();
        Task<(bool Success, bool IsFavorite, string Message)> ToggleFavoriteAsync(int productId);
        Task<(bool Success, string Message)> DeleteFavoriteAsync(int id);
    }
}