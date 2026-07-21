using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.Threading.Tasks;
using Inventory.Core.DTOs;

namespace Inventory.Core.Interfaces
{
    public interface IProductService
    {
        Task<IReadOnlyList<ProductResponseDto>> GetAllProductsAsync();
        Task<ProductResponseDto> GetProductByIdAsync(int id);
        Task<ProductResponseDto> CreateProductAsync(ProductRequestDto model, IFormFile? imgFile = null);
        Task<ProductResponseDto> UpdateProductAsync(int id, ProductRequestDto model, IFormFile? imgFile = null);
        Task<bool> DeleteProductAsync(int id);
    }
}