using Inventory.Core.DTOs;
using Inventory.Core.Entities.Favorites;
using Inventory.Core.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Inventory.Core.Services
{
    public class FavoriteService : IFavoriteService
    {
        private readonly IRepository<Favorite> _favoriteRepo;

        public FavoriteService(IRepository<Favorite> favoriteRepo)
        {
            _favoriteRepo = favoriteRepo;
        }

        public async Task<IReadOnlyList<FavoriteResponseDto>> GetAllFavoritesAsync()
        {
            var favorites = await _favoriteRepo.GetAllDtoAsync<FavoriteResponseDto>(
                include: q => q.Include(f => f.Product)
            );

            return favorites.Where(f => f.IsFavorite).ToList();
        }

        public async Task<FavoriteResponseDto?> ToggleFavoriteAsync(int productId)
        {
            var fav = await _favoriteRepo.GetFirstOrDefaultAsync(
                predicate: f => f.ProductId == productId,
                include: q => q.Include(f => f.Product)
            );

            if (fav != null)
            {
                fav.IsFavorite = !fav.IsFavorite;
                _favoriteRepo.Update(fav);
            }
            else
            {
                fav = new Favorite
                {
                    ProductId = productId,
                    IsFavorite = true
                };

                await _favoriteRepo.AddAsync(fav);
            }

            await _favoriteRepo.SaveChangesAsync();

            return await _favoriteRepo.GetDtoByIdAsync<FavoriteResponseDto>(fav.Id);
        }

        public async Task<bool> DeleteFavoriteAsync(int id)
        {
            return await _favoriteRepo.DeleteAndSaveAsync(id);
        }

        public async Task<bool> IsProductFavoriteAsync(int productId)
        {
            var fav = await _favoriteRepo.GetFirstOrDefaultAsync(f => f.ProductId == productId && f.IsFavorite);
            return fav != null;
        }
    }
}