using Inventory.Web.ViewModels.Categories;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Inventory.Web.Interfaces
{
    public interface IViewCategoryService
    {
        Task<List<CategoryVM>> GetAllCategoriesAsync();
        Task<CategoryDetailsVM?> GetCategoryDetailsAsync(int id);
        Task<CreateUpdateCategoryVM?> GetCategoryForEditAsync(int id);
        Task<(bool Success, string Message)> CreateCategoryAsync(CreateUpdateCategoryVM model);
        Task<(bool Success, string Message)> UpdateCategoryAsync(int id, CreateUpdateCategoryVM model);
        Task<(bool Success, string Message)> DeleteCategoryAsync(int id);
        Task<string> SaveImageLocallyAsync(IFormFile file);
        void DeleteLocalImage(string relativePath);
    }
}