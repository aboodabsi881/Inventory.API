using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.Threading.Tasks;
using Inventory.Core.DTOs;

namespace Inventory.Core.Interfaces
{
    public interface ICategoryService
    {
        Task<List<CategoryResponseDto>> GetAllCategoriesAsync();
        Task<CategoryResponseDto> GetCategoryByIdAsync(int id);
        Task<CategoryResponseDto> CreateCategoryAsync(CategoryRequestDto model);
        Task<CategoryResponseDto> UpdateCategoryAsync(int id, CategoryRequestDto model);
        Task<bool> DeleteCategoryAsync(int id);
        Task<CategoryIndexResponseDto> GetCategoriesIndexAsync();
    }
}