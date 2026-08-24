using Inventory.Core.DTOs;
using Inventory.Core.Entities.Products;
using Inventory.Core.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Inventory.Core.Services
{
    public class ProductService : IProductService
    {
        private readonly IRepository<Product> _productRepo;

        public ProductService(IRepository<Product> productRepo)
        {
            _productRepo = productRepo;
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
    }
}