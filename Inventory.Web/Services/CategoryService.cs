using Inventory.Web.Interfaces;
using Inventory.Web.ViewModels.Carts;
using Inventory.Web.ViewModels.Categories;
using Inventory.Web.ViewModels.Favorites;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;

namespace Inventory.Web.Services
{
    public class ViewCategoryService : IViewCategoryService
    {
        private readonly HttpClient _client;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

        public ViewCategoryService(IHttpClientFactory httpClientFactory, IWebHostEnvironment webHostEnvironment)
        {
            _client = httpClientFactory.CreateClient("InventoryAPI");
            _webHostEnvironment = webHostEnvironment;
        }

        public async Task<List<CategoryVM>> GetAllCategoriesAsync()
        {
            try
            {
                return await _client.GetFromJsonAsync<List<CategoryVM>>("Categories", JsonOptions)
                       ?? new List<CategoryVM>();
            }
            catch
            {
                return new List<CategoryVM>();
            }
        }

        public async Task<CategoryDetailsVM?> GetCategoryDetailsAsync(int id)
        {
            try
            {
                var category = await _client.GetFromJsonAsync<CategoryDetailsVM>($"Categories/{id}", JsonOptions);
                if (category == null) return null;

                List<FavoriteVM> favorites = new();
                List<CartVM> carts = new();

                try
                {
                    favorites = await _client.GetFromJsonAsync<List<FavoriteVM>>("Favorites", JsonOptions) ?? new List<FavoriteVM>();
                    carts = await _client.GetFromJsonAsync<List<CartVM>>("Carts", JsonOptions) ?? new List<CartVM>();
                }
                catch
                {
                }

                var favoriteProductIds = favorites.Where(f => f.IsFavorite).Select(f => f.ProductId).ToHashSet();
                var cartDictionary = carts.ToDictionary(c => c.ProductId, c => c.Quantity);

                if (category.ProductsVM != null)
                {
                    foreach (var product in category.ProductsVM)
                    {
                        product.IsFavorite = favoriteProductIds.Contains(product.Id);
                        if (cartDictionary.TryGetValue(product.Id, out int qty))
                        {
                            product.QuantityInCart = qty;
                        }
                    }
                }

                return category;
            }
            catch
            {
                return null;
            }
        }

        public async Task<CreateUpdateCategoryVM?> GetCategoryForEditAsync(int id)
        {
            try
            {
                return await _client.GetFromJsonAsync<CreateUpdateCategoryVM>($"Categories/{id}", JsonOptions);
            }
            catch
            {
                return null;
            }
        }

        public async Task<(bool Success, string Message)> CreateCategoryAsync(CreateUpdateCategoryVM model)
        {
            string? imagePath = model.ImgFile is { Length: > 0 }
                ? await SaveImageLocallyAsync(model.ImgFile)
                : null;

            using var content = new MultipartFormDataContent
            {
                { new StringContent(model.Name ?? string.Empty), "Name" },
                { new StringContent(imagePath ?? string.Empty), "Img" }
            };

            var response = await _client.PostAsync("Categories", content);
            if (response.IsSuccessStatusCode)
            {
                return (true, string.Empty);
            }

            if (!string.IsNullOrEmpty(imagePath))
                DeleteLocalImage(imagePath);

            var errorDetails = await response.Content.ReadAsStringAsync();
            return (false, $"API Error: {errorDetails}");
        }

        public async Task<(bool Success, string Message)> UpdateCategoryAsync(int id, CreateUpdateCategoryVM model)
        {
            string? updatedImagePath = model.Img;

            if (model.ImgFile is { Length: > 0 })
            {
                if (!string.IsNullOrEmpty(model.Img))
                    DeleteLocalImage(model.Img);

                updatedImagePath = await SaveImageLocallyAsync(model.ImgFile);
            }

            using var content = new MultipartFormDataContent
            {
                { new StringContent(model.Id.ToString()), "Id" },
                { new StringContent(model.Name ?? string.Empty), "Name" },
                { new StringContent(updatedImagePath ?? string.Empty), "Img" }
            };

            var response = await _client.PutAsync($"Categories/{id}", content);
            if (response.IsSuccessStatusCode)
            {
                return (true, string.Empty);
            }

            var errorDetails = await response.Content.ReadAsStringAsync();
            return (false, $"API Error: {errorDetails}");
        }

        public async Task<(bool Success, string Message)> DeleteCategoryAsync(int id)
        {
            var category = await _client.GetFromJsonAsync<CategoryDetailsVM>($"Categories/{id}", JsonOptions);
            var response = await _client.DeleteAsync($"Categories/{id}");

            if (response.IsSuccessStatusCode)
            {
                if (category != null && !string.IsNullOrEmpty(category.Img))
                    DeleteLocalImage(category.Img);

                return (true, string.Empty);
            }

            return (false, "Failed to delete category via API.");
        }

        public async Task<string> SaveImageLocallyAsync(IFormFile file)
        {
            string folderPath = Path.Combine(_webHostEnvironment.WebRootPath, "images", "categories");
            if (!Directory.Exists(folderPath))
                Directory.CreateDirectory(folderPath);

            string uniqueFileName = $"{Guid.NewGuid()}_{Path.GetFileName(file.FileName)}";
            string fullPath = Path.Combine(folderPath, uniqueFileName);

            await using var stream = new FileStream(fullPath, FileMode.Create);
            await file.CopyToAsync(stream);

            return $"/images/categories/{uniqueFileName}";
        }

        public void DeleteLocalImage(string relativePath)
        {
            if (string.IsNullOrWhiteSpace(relativePath)) return;

            string fullPath = Path.Combine(_webHostEnvironment.WebRootPath, relativePath.TrimStart('/'));
            if (File.Exists(fullPath))
            {
                File.Delete(fullPath);
            }
        }
    }
}