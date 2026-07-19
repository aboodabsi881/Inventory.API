using AutoMapper;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Inventory.Core.DTOs;
using Inventory.Core.Entities.Carts;
using Inventory.Core.Entities.Categories;
using Inventory.Core.Entities.Favorites;
using Inventory.Core.Interfaces;

namespace Inventory.Data.Services
{
    public class CategoryService : ICategoryService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IWebHostEnvironment _env;
        private readonly IMapper _mapper;

        public CategoryService(IUnitOfWork unitOfWork, IWebHostEnvironment env, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _env = env;
            _mapper = mapper;
        }

        public async Task<IReadOnlyList<CategoryResponseDto>> GetAllCategoriesAsync()
        {
            var categories = await _unitOfWork.Repository<Category>().GetAllAsync();
            return _mapper.Map<IReadOnlyList<CategoryResponseDto>>(categories);
        }

        public async Task<CategoryResponseDto> GetCategoryByIdAsync(int id)
        {
            var category = await _unitOfWork.Repository<Category>().GetByIdAsync(id);
            if (category == null)
                throw new KeyNotFoundException($"Category with ID {id} was not found.");

            return _mapper.Map<CategoryResponseDto>(category);
        }

        public async Task<CategoryResponseDto> CreateCategoryAsync(CategoryRequestDto model, IFormFile? imgFile = null)
        {
            var category = _mapper.Map<Category>(model);

            if (imgFile != null)
                category.Img = await SaveImageAsync(imgFile);

            await _unitOfWork.Repository<Category>().AddAsync(category);
            await _unitOfWork.CompleteAsync();

            return _mapper.Map<CategoryResponseDto>(category);
        }

        public async Task<CategoryResponseDto> UpdateCategoryAsync(int id, CategoryRequestDto model, IFormFile? imgFile = null)
        {
            var existing = await _unitOfWork.Repository<Category>().GetByIdAsync(id);
            if (existing == null)
                throw new KeyNotFoundException($"Category with ID {id} was not found.");

            // Merge values from request payload into tracked DB entity instance
            _mapper.Map(model, existing);

            if (imgFile != null)
                existing.Img = await SaveImageAsync(imgFile);

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
            // Fetch everything cleanly using individual entity repositories via IUnitOfWork
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

        private async Task<string> SaveImageAsync(IFormFile imgFile)
        {
            var fileName = $"{Path.GetFileNameWithoutExtension(imgFile.FileName)}_{Guid.NewGuid()}{Path.GetExtension(imgFile.FileName)}";
            var directoryPath = Path.Combine(_env.WebRootPath, "categories");

            // Ensure physical disk directory is ready before streaming data
            if (!Directory.Exists(directoryPath))
                Directory.CreateDirectory(directoryPath);

            var fullPath = Path.Combine(directoryPath, fileName);

            await using (var stream = new FileStream(fullPath, FileMode.Create))
            {
                await imgFile.CopyToAsync(stream);
            }

            return "/categories/" + fileName;
        }
    }
}