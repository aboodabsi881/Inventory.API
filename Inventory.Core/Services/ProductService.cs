using AutoMapper;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Inventory.Core.DTOs;
using Inventory.Core.Entities.Products;
using Inventory.Core.Interfaces;

namespace Inventory.Core.Services
{
    public class ProductService : IProductService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IWebHostEnvironment _env;
        private readonly IMapper _mapper;

        public ProductService(IUnitOfWork unitOfWork, IWebHostEnvironment env, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _env = env;
            _mapper = mapper;
        }

        public async Task<IReadOnlyList<ProductResponseDto>> GetAllProductsAsync()
        {
            var products = await _unitOfWork.Repository<Product>().GetAllAsync();
            return _mapper.Map<IReadOnlyList<ProductResponseDto>>(products);
        }

        public async Task<ProductResponseDto> GetProductByIdAsync(int id)
        {
            var product = await _unitOfWork.Repository<Product>().GetByIdAsync(id);
            if (product == null)
                throw new KeyNotFoundException($"Product with ID {id} was not found.");

            return _mapper.Map<ProductResponseDto>(product);
        }

        public async Task<ProductResponseDto> CreateProductAsync(ProductRequestDto model, IFormFile? imgFile = null)
        {
            var product = _mapper.Map<Product>(model);

            if (imgFile != null && imgFile.Length > 0)
            {
                product.Img = await SaveImageAsync(imgFile);
            }
            else
            {
                product.Img = "/products/default.png"; // Default fallback image
            }

            await _unitOfWork.Repository<Product>().AddAsync(product);
            await _unitOfWork.CompleteAsync();

            return _mapper.Map<ProductResponseDto>(product);
        }

        public async Task<ProductResponseDto> UpdateProductAsync(int id, ProductRequestDto model, IFormFile? imgFile = null)
        {
            var existingProduct = await _unitOfWork.Repository<Product>().GetByIdAsync(id);
            if (existingProduct == null)
                throw new KeyNotFoundException($"Product with ID {id} was not found.");

            // Overwrite updated values onto tracked DB entity state
            _mapper.Map(model, existingProduct);

            if (imgFile != null && imgFile.Length > 0)
            {
                existingProduct.Img = await SaveImageAsync(imgFile);
            }

            _unitOfWork.Repository<Product>().Update(existingProduct);
            await _unitOfWork.CompleteAsync();

            return _mapper.Map<ProductResponseDto>(existingProduct);
        }

        public async Task<bool> DeleteProductAsync(int id)
        {
            var product = await _unitOfWork.Repository<Product>().GetByIdAsync(id);
            if (product == null)
                throw new KeyNotFoundException($"Product with ID {id} was not found.");

            _unitOfWork.Repository<Product>().Delete(product);
            var result = await _unitOfWork.CompleteAsync();

            return result > 0;
        }

        private async Task<string> SaveImageAsync(IFormFile imgFile)
        {
            var fileName = $"{Guid.NewGuid()}{Path.GetExtension(imgFile.FileName)}";
            var directoryPath = Path.Combine(_env.WebRootPath, "products");

            // Ensure physical directory exists in wwwroot/products
            if (!Directory.Exists(directoryPath))
                Directory.CreateDirectory(directoryPath);

            var fullPath = Path.Combine(directoryPath, fileName);

            await using (var stream = new FileStream(fullPath, FileMode.Create))
            {
                await imgFile.CopyToAsync(stream);
            }

            return "/products/" + fileName;
        }
    }
}