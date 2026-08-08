using Inventory.Core.DTOs;
using Inventory.Core.Entities.Carts;
using Inventory.Core.Interfaces;
using Microsoft.EntityFrameworkCore;

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
            var cartItem = await _cartRepo.GetFirstOrDefaultAsync(
                predicate: c => c.ProductId == productId,
                include: q => q.Include(c => c.Product)
            );

            if (cartItem == null)
            {
                if (change <= 0) change = 1;

                cartItem = new Cart
                {
                    ProductId = productId,
                    Quantity = change
                };

                await _cartRepo.AddAsync(cartItem);
            }
            else
            {
                cartItem.Quantity += change;

                if (cartItem.Quantity <= 0)
                {
                    _cartRepo.Delete(cartItem);
                    await _cartRepo.SaveChangesAsync();
                    return null;
                }

                _cartRepo.Update(cartItem);
            }

            await _cartRepo.SaveChangesAsync();

            return await _cartRepo.GetDtoByIdAsync<CartResponseDto>(cartItem.Id);
        }

        public async Task<bool> RemoveItemAsync(int cartId)
        {
            return await _cartRepo.DeleteAndSaveAsync(cartId);
        }

        public async Task<decimal> GetCartTotalAsync()
        {
            var cartItems = await _cartRepo.GetAllAsync(include: q => q.Include(c => c.Product));
            return cartItems.Sum(item => item.Quantity * (item.Product?.Price ?? 0));
        }
    }
}