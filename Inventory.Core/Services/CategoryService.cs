using Inventory.Core.DTOs;
using Inventory.Core.Entities.Categories;
using Inventory.Core.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Inventory.Core.Services
{
    public class CategoryService : ICategoryService
    {
        private readonly IRepository<Category> _categoryRepo;

        public CategoryService(IRepository<Category> categoryRepo)
        {
            _categoryRepo = categoryRepo;
        }

        public async Task<List<CategoryResponseDto>> GetAllCategoriesAsync()
        {
            var categories = await _categoryRepo.GetAllDtoAsync<CategoryResponseDto>(
                include: q => q.Include(c => c.Products)
            );

            return new List<CategoryResponseDto>(categories);
        }

        public async Task<CategoryResponseDto?> GetCategoryByIdAsync(int id)
        {
            return await _categoryRepo.GetDtoByIdAsync<CategoryResponseDto>(
                id,
                include: q => q.Include(c => c.Products)
            );
        }

        public async Task<CategoryResponseDto> CreateCategoryAsync(CategoryRequestDto model)
        {
            return await _categoryRepo.CreateFromDtoAsync<CategoryRequestDto, CategoryResponseDto>(model);
        }

        public async Task<CategoryResponseDto> UpdateCategoryAsync(int id, CategoryRequestDto model)
        {
            return await _categoryRepo.UpdateFromDtoAsync<CategoryRequestDto, CategoryResponseDto>(id, model);
        }

        public async Task<bool> DeleteCategoryAsync(int id)
        {
            return await _categoryRepo.DeleteAndSaveAsync(id);
        }

        public async Task<CategoryIndexResponseDto> GetCategoriesIndexAsync()
        {
            var categories = await _categoryRepo.GetAllDtoAsync<CategoryResponseDto>();

            return new CategoryIndexResponseDto
            {
                Categories = categories
            };
        }
    }
}