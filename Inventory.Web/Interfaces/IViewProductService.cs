using Inventory.Web.ViewModels.Categories;
using Inventory.Web.ViewModels.Products;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Inventory.Web.Interfaces
{
    public interface IViewProductService
    {
        Task<List<ProductVM>> GetAllProductsAsync();
        Task<ProductDetailsVM?> GetProductDetailsAsync(int id);
        Task<CreateUpdateProductVM?> GetProductForEditAsync(int id);
        Task<SelectList> GetCategoriesSelectListAsync(object? selectedCategory = null);
        Task<(bool Success, string Message)> CreateProductAsync(CreateUpdateProductVM model);
        Task<(bool Success, string Message)> UpdateProductAsync(int id, CreateUpdateProductVM model);
        Task<(bool Success, string Message)> DeleteProductAsync(int id);
        Task<string> SaveImageLocallyAsync(IFormFile file);
        void DeleteLocalImage(string relativePath);
    }
}