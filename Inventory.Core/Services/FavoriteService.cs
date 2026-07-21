using AutoMapper;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Inventory.Core.DTOs;
using Inventory.Core.Entities.Favorites;
using Inventory.Core.Entities.Products;
using Inventory.Core.Interfaces;

namespace Inventory.Core.Services
{
    public class FavoriteService : IFavoriteService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public FavoriteService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<IReadOnlyList<FavoriteResponseDto>> GetAllFavoritesAsync()
        {
            var favorites = await _unitOfWork.Repository<Favorite>().GetAllAsync();
            var products = await _unitOfWork.Repository<Product>().GetAllAsync();

            // Link products so AutoMapper can map Product Name/Image if needed by the DTO
            foreach (var fav in favorites)
            {
                fav.Product = products.FirstOrDefault(p => p.Id == fav.ProductId);
            }

            return _mapper.Map<IReadOnlyList<FavoriteResponseDto>>(favorites);
        }

        public async Task<FavoriteResponseDto?> ToggleFavoriteAsync(int productId)
        {
            var product = await _unitOfWork.Repository<Product>().GetByIdAsync(productId);
            if (product == null)
                return null;

            var allFavorites = await _unitOfWork.Repository<Favorite>().GetAllAsync();
            var fav = allFavorites.FirstOrDefault(f => f.ProductId == productId);

            if (fav != null)
            {
                // Toggle the existing favorite state
                fav.IsFavorite = !fav.IsFavorite;
                _unitOfWork.Repository<Favorite>().Update(fav);
            }
            else
            {
                // Create a new favorite record
                fav = new Favorite
                {
                    ProductId = productId,
                    IsFavorite = true
                };

                await _unitOfWork.Repository<Favorite>().AddAsync(fav);
            }

            await _unitOfWork.CompleteAsync();

            fav.Product = product; // Attach product reference for mapping
            return _mapper.Map<FavoriteResponseDto>(fav);
        }

        public async Task<bool> DeleteFavoriteAsync(int id)
        {
            var fav = await _unitOfWork.Repository<Favorite>().GetByIdAsync(id);
            if (fav == null) return false;

            _unitOfWork.Repository<Favorite>().Delete(fav);
            var result = await _unitOfWork.CompleteAsync();

            return result > 0;
        }

        public async Task<bool> IsProductFavoriteAsync(int productId)
        {
            var allFavorites = await _unitOfWork.Repository<Favorite>().GetAllAsync();
            return allFavorites.Any(f => f.ProductId == productId && f.IsFavorite);
        }
    }
}