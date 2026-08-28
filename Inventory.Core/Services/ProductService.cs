using Inventory.Core.DTOs;
using Inventory.Core.Entities.Products;
using Inventory.Core.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Inventory.Core.Services
{
    public class ProductService : IProductService
    {
        private readonly IRepository<Product> _productRepo;
        private readonly ICurrentUserService _currentUser;

        public ProductService(IRepository<Product> productRepo, ICurrentUserService currentUser)
        {
            _productRepo = productRepo;
            _currentUser = currentUser;
        }

        public async Task<IReadOnlyList<ProductResponseDto>> GetAllProductsDtoAsync()
        {
            return await _productRepo.GetAllDtoAsync<ProductResponseDto>(
                include: q => q.Include(p => p.Category)
            );
        }

        public async Task<ProductResponseDto?> GetProductDtoByIdAsync(int id)
        {
            return await _productRepo.GetDtoByIdAsync<ProductResponseDto>(
                id,
                include: q => q.Include(p => p.Category)
            );
        }

        public async Task<ProductResponseDto> CreateProductAsync(ProductRequestDto model)
        {
            return await _productRepo.CreateFromDtoAsync<ProductRequestDto, ProductResponseDto>(model);
        }

        public async Task<ProductResponseDto> UpdateProductAsync(int id, ProductRequestDto model)
        {
            return await _productRepo.UpdateFromDtoAsync<ProductRequestDto, ProductResponseDto>(id, model);
        }

        public async Task<bool> DeleteProductAsync(int id)
        {
            return await _productRepo.DeleteAndSaveAsync(id);
        }

        public async Task<bool> UpdateStockAsync(int productId, int quantityChange)
        {
            var product = await _productRepo.GetByIdAsync(productId);
            if (product == null) return false;

            int newQuantity = product.Quantity + quantityChange;
            if (newQuantity < 0)
            {
                throw new InvalidOperationException($"Insufficient stock. Current available quantity is {product.Quantity}.");
            }

            product.Quantity = newQuantity;
            _productRepo.Update(product);
            return await _productRepo.SaveChangesAsync() > 0;
        }
    }
}