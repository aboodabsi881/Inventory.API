using AutoMapper;
using Inventory.Core.DTOs;
using Inventory.Core.Entities.Carts;
using Inventory.Core.Entities.Categories;
using Inventory.Core.Entities.Favorites;
using Inventory.Core.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Inventory.Data.Services
{
    public class CategoryService : ICategoryService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public CategoryService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<List<CategoryResponseDto>> GetAllCategoriesAsync()
        {
            var categories = await _unitOfWork.Repository<Category>()
                .GetAllAsync(include: q => q.Include(c => c.Products)); 

            return _mapper.Map<List<CategoryResponseDto>>(categories);
        }

        public async Task<CategoryResponseDto?> GetCategoryByIdAsync(int id)
        {
            var category = await _unitOfWork.Repository<Category>()
                .GetFirstOrDefaultAsync(
                    predicate: c => c.Id == id,
                    include: q => q.Include(c => c.Products) // 👈 Loads products from DB
                );

            if (category == null)
                return null;

            return _mapper.Map<CategoryResponseDto>(category);
        }

        // 💡 تم إلغاء التعامل مع IFormFile واستقبال المسار النصي للصورة من model.Img مباشرة
        public async Task<CategoryResponseDto> CreateCategoryAsync(CategoryRequestDto model)
        {
            var category = _mapper.Map<Category>(model);

            await _unitOfWork.Repository<Category>().AddAsync(category);
            await _unitOfWork.CompleteAsync();

            return _mapper.Map<CategoryResponseDto>(category);
        }

        public async Task<CategoryResponseDto> UpdateCategoryAsync(int id, CategoryRequestDto model)
        {
            var existing = await _unitOfWork.Repository<Category>().GetByIdAsync(id);
            if (existing == null)
                throw new KeyNotFoundException($"Category with ID {id} was not found.");

            // دمج القيم القادمة من Request DTO مع الكائن الموجود بحافظة قاعدة البيانات
            _mapper.Map(model, existing);

            _unitOfWork.Repository<Category>().Update(existing);
            await _unitOfWork.CompleteAsync();

            return _mapper.Map<CategoryResponseDto>(existing);
        }

        public async Task<bool> DeleteCategoryAsync(int id)
        {
            var category = await _unitOfWork.Repository<Category>().GetByIdAsync(id);
            if (category == null)
                throw new KeyNotFoundException($"Category with ID {id} was not found.");

            _unitOfWork.Repository<Category>().Delete(category);
            var result = await _unitOfWork.CompleteAsync();

            return result > 0;
        }

        public async Task<CategoryIndexResponseDto> GetCategoriesIndexAsync()
        {
            var categories = await _unitOfWork.Repository<Category>().GetAllAsync();
            var favorites = await _unitOfWork.Repository<Favorite>().GetAllAsync();
            var cartItems = await _unitOfWork.Repository<Cart>().GetAllAsync();

            return new CategoryIndexResponseDto
            {
                Categories = _mapper.Map<IReadOnlyList<CategoryResponseDto>>(categories),
                Favorites = _mapper.Map<IReadOnlyList<FavoriteResponseDto>>(favorites),
                CartItems = _mapper.Map<IReadOnlyList<CartResponseDto>>(cartItems)
            };
        }
    }
}