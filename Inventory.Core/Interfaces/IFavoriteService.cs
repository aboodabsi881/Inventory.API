using System.Collections.Generic;
using System.Threading.Tasks;
using Inventory.Core.DTOs;

namespace Inventory.Core.Interfaces
{
    public interface IFavoriteService
    {
        Task<IReadOnlyList<FavoriteResponseDto>> GetAllFavoritesAsync();

        // Renamed from AddFavoriteAsync to accurately reflect your toggle logic
        Task<FavoriteResponseDto?> ToggleFavoriteAsync(int productId);

        Task<bool> DeleteFavoriteAsync(int id);
        Task<bool> IsProductFavoriteAsync(int productId);
    }
}