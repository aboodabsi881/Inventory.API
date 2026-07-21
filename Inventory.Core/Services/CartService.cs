using AutoMapper;
using Inventory.Core.DTOs;
using Inventory.Core.Entities.Carts;
using Inventory.Core.Entities.Products;
using Inventory.Core.Interfaces;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Inventory.Core.Services
{
    public class CartService : ICartService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public CartService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<IReadOnlyList<CartResponseDto>> GetCartAsync()
        {
            var cartItems = await _unitOfWork.Repository<Cart>().GetAllAsync();
            var products = await _unitOfWork.Repository<Product>().GetAllAsync();

            var productDict = products.ToDictionary(p => p.Id);

            foreach (var item in cartItems)
            {
                if (productDict.TryGetValue(item.ProductId, out var product))
                {
                    item.Product = product;
                    item.TotalPrice = item.Quantity * product.Price;
                }
            }

            return _mapper.Map<IReadOnlyList<CartResponseDto>>(cartItems);
        }

        public async Task<CartResponseDto?> AddOrUpdateItemAsync(int productId, int change)
        {
            var product = await _unitOfWork.Repository<Product>().GetByIdAsync(productId);
            if (product == null)
                throw new KeyNotFoundException($"Product with ID {productId} was not found.");

            var allCartItems = await _unitOfWork.Repository<Cart>().GetAllAsync();
            var cartItem = allCartItems.FirstOrDefault(c => c.ProductId == productId);

            if (cartItem == null)
            {
                if (change <= 0) change = 1;

                cartItem = new Cart
                {
                    ProductId = productId,
                    Quantity = change,
                    TotalPrice = change * product.Price
                };

                await _unitOfWork.Repository<Cart>().AddAsync(cartItem);
            }
            else
            {
                cartItem.Quantity += change;

                if (cartItem.Quantity <= 0)
                {
                    _unitOfWork.Repository<Cart>().Delete(cartItem);
                    await _unitOfWork.CompleteAsync();
                    return null;
                }

                cartItem.TotalPrice = cartItem.Quantity * product.Price;
                _unitOfWork.Repository<Cart>().Update(cartItem);
            }

            await _unitOfWork.CompleteAsync();

            // Re-attach product for mapping to DTO
            cartItem.Product = product;
            return _mapper.Map<CartResponseDto>(cartItem);
        }

        public async Task<bool> RemoveItemAsync(int cartId)
        {
            var cartItem = await _unitOfWork.Repository<Cart>().GetByIdAsync(cartId);
            if (cartItem == null) return false;

            _unitOfWork.Repository<Cart>().Delete(cartItem);
            var result = await _unitOfWork.CompleteAsync();

            return result > 0;
        }

        public async Task<decimal> GetCartTotalAsync()
        {
            var cartItems = await _unitOfWork.Repository<Cart>().GetAllAsync();
            var products = await _unitOfWork.Repository<Product>().GetAllAsync();

            var productDict = products.ToDictionary(p => p.Id);

            decimal total = 0;
            foreach (var item in cartItems)
            {
                if (productDict.TryGetValue(item.ProductId, out var product))
                {
                    total += item.Quantity * product.Price;
                }
            }

            return total;
        }
    }
}