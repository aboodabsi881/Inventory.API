using Inventory.Core.DTOs;
using Inventory.Core.Entities.Carts;
using Inventory.Core.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Inventory.Core.Services
{
    public class CartService : ICartService
    {
        private readonly IRepository<Cart> _cartRepo;

        public CartService(IRepository<Cart> cartRepo)
        {
            _cartRepo = cartRepo;
        }

        public async Task<IReadOnlyList<CartResponseDto>> GetCartAsync()
        {
            return await _cartRepo.GetAllDtoAsync<CartResponseDto>(
                include: q => q.Include(c => c.Product)
            );
        }

        public async Task<CartResponseDto?> AddOrUpdateItemAsync(int productId, int change)
        {
            return await _cartRepo.AddOrUpdateCartItemAsync(productId, change);
        }

        public async Task<bool> RemoveItemAsync(int cartId)
        {
            return await _cartRepo.DeleteAndSaveAsync(cartId);
        }

        public async Task<decimal> GetCartTotalAsync()
        {
            return await _cartRepo.GetCartTotalAsync();
        }
    }
}