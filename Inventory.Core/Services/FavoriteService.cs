using Inventory.Core.DTOs;
using Inventory.Core.Entities.Favorites;
using Inventory.Core.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Inventory.Core.Services
{
    public class FavoriteService : IFavoriteService
    {
        private readonly IRepository<Favorite> _favoriteRepo;
        private readonly ICurrentUserService _currentUser;

        public FavoriteService(IRepository<Favorite> favoriteRepo, ICurrentUserService currentUser)
        {
            _favoriteRepo = favoriteRepo;
            _currentUser = currentUser;
        }

        private int GetCurrentUserId()
        {
            return _currentUser.UserId
                ?? throw new UnauthorizedAccessException("User is not authenticated.");
        }

        public async Task<IReadOnlyList<FavoriteResponseDto>> GetAllFavoritesAsync()
        {
            int userId = GetCurrentUserId();

            var favorites = await _favoriteRepo.GetAllDtoAsync<FavoriteResponseDto>(
                include: q => q.Where(f => f.UserId == userId && f.IsFavorite).Include(f => f.Product)
            );

            return favorites;
        }

        public async Task<FavoriteResponseDto?> ToggleFavoriteAsync(int productId)
        {
            int userId = GetCurrentUserId();

            var fav = await _favoriteRepo.GetFirstOrDefaultAsync(
                predicate: f => f.ProductId == productId && f.UserId == userId,
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
                    UserId = userId, 
                    IsFavorite = true
                };

                await _favoriteRepo.AddAsync(fav);
            }

            await _favoriteRepo.SaveChangesAsync();

            return await _favoriteRepo.GetDtoByIdAsync<FavoriteResponseDto>(fav.Id);
        }

        public async Task<bool> DeleteFavoriteAsync(int id)
        {
            int userId = GetCurrentUserId();

            var fav = await _favoriteRepo.GetFirstOrDefaultAsync(
                f => f.Id == id && f.UserId == userId
            );

            if (fav == null)
            {
                return false;
            }

            _favoriteRepo.Delete(fav);
            return await _favoriteRepo.SaveChangesAsync() > 0;
        }

        public async Task<bool> IsProductFavoriteAsync(int productId)
        {
            int userId = GetCurrentUserId();

            var fav = await _favoriteRepo.GetFirstOrDefaultAsync(
                f => f.ProductId == productId && f.UserId == userId && f.IsFavorite
            );

            return fav != null;
        }
    }
}