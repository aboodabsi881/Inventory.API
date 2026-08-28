using Inventory.Core.DTOs;
using Inventory.Core.Entities.Carts;
using Inventory.Core.Entities.Products;
using Inventory.Core.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Inventory.Core.Services
{
    public class CartService : ICartService
    {
        private readonly IRepository<Cart> _cartRepo;
        private readonly IRepository<Product> _productRepo;
        private readonly ICurrentUserService _currentUser;

        public CartService(
            IRepository<Cart> cartRepo,
            IRepository<Product> productRepo,
            ICurrentUserService currentUser)
        {
            _cartRepo = cartRepo;
            _productRepo = productRepo;
            _currentUser = currentUser;
        }

        private int GetCurrentUserId()
        {
            return _currentUser.UserId
                ?? throw new UnauthorizedAccessException("User is not authenticated.");
        }

        public async Task<IReadOnlyList<CartResponseDto>> GetCartAsync()
        {
            int userId = GetCurrentUserId();

            return await _cartRepo.GetAllDtoAsync<CartResponseDto>(
                include: q => q.Where(c => c.UserId == userId).Include(c => c.Product)
            );
        }

        public async Task<CartResponseDto?> AddOrUpdateItemAsync(int productId, int change)
        {
            int userId = GetCurrentUserId();

            var product = await _productRepo.GetByIdAsync(productId);
            if (product == null)
            {
                throw new KeyNotFoundException($"Product with ID {productId} was not found.");
            }

            var cartItem = await _cartRepo.GetFirstOrDefaultAsync(
                predicate: c => c.ProductId == productId && c.UserId == userId
            );

            // 3. Calculate target quantity
            int targetQuantity = (cartItem?.Quantity ?? 0) + (change == 0 ? 1 : change);

            if (targetQuantity <= 0)
            {
                if (cartItem != null)
                {
                    _cartRepo.Delete(cartItem);
                    await _cartRepo.SaveChangesAsync();
                }
                return null;
            }

            if (targetQuantity > product.Quantity)
            {
                throw new InvalidOperationException(
                    $"Cannot add {targetQuantity} items. Only {product.Quantity} available in stock.");
            }

            if (cartItem == null)
            {
                cartItem = new Cart
                {
                    ProductId = productId,
                    Quantity = targetQuantity,
                    UserId = userId
                };

                await _cartRepo.AddAsync(cartItem);
            }
            else
            {
                cartItem.Quantity = targetQuantity;
                _cartRepo.Update(cartItem);
            }

            await _cartRepo.SaveChangesAsync();

            return await _cartRepo.GetDtoByIdAsync<CartResponseDto>(cartItem.Id);
        }

        public async Task<bool> RemoveItemAsync(int cartId)
        {
            int userId = GetCurrentUserId();

            var cartItem = await _cartRepo.GetFirstOrDefaultAsync(
                c => c.Id == cartId && c.UserId == userId
            );

            if (cartItem == null)
            {
                return false;
            }

            _cartRepo.Delete(cartItem);
            return await _cartRepo.SaveChangesAsync() > 0;
        }

        public async Task<decimal> GetCartTotalAsync()
        {
            int userId = GetCurrentUserId();

            var cartItems = await _cartRepo.GetAllAsync(
                include: q => q.Where(c => c.UserId == userId).Include(c => c.Product)
            );

            return cartItems.Sum(item => item.Quantity * (item.Product?.Price ?? 0));
        }
    }
}