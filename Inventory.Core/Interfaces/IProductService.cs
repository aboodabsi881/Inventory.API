using Inventory.Core.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Inventory.Core.Interfaces
{
    public interface IProductService
    {
        Task<IReadOnlyList<ProductResponseDto>> GetAllProductsDtoAsync();
        Task<ProductResponseDto?> GetProductDtoByIdAsync(int id);
        Task<ProductResponseDto> CreateProductAsync(ProductRequestDto model);
        Task<ProductResponseDto> UpdateProductAsync(int id, ProductRequestDto model);
        Task<bool> DeleteProductAsync(int id);
    }
}