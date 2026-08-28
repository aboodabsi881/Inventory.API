using Inventory.Web.Interfaces;
using Inventory.Web.ViewModels.Carts;
using Inventory.Web.ViewModels.Categories;
using Inventory.Web.ViewModels.Favorites;
using Inventory.Web.ViewModels.Products;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;

namespace Inventory.Web.Services
{
    public class ViewProductService : IViewProductService
    {
        private readonly HttpClient _client;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

        public ViewProductService(IHttpClientFactory httpClientFactory, IWebHostEnvironment webHostEnvironment)
        {
            _client = httpClientFactory.CreateClient("InventoryAPI");
            _webHostEnvironment = webHostEnvironment;
        }

        public async Task<List<ProductVM>> GetAllProductsAsync()
        {
            List<ProductVM> products = new();
            List<FavoriteVM> favorites = new();
            List<CartVM> cartItems = new();

            try
            {
                products = await _client.GetFromJsonAsync<List<ProductVM>>("Products", JsonOptions) ?? new();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ViewProductService] Error loading products: {ex.Message}");
                return new List<ProductVM>();
            }

            try
            {
                favorites = await _client.GetFromJsonAsync<List<FavoriteVM>>("Favorites", JsonOptions) ?? new();
            }
            catch {  }

            try
            {
                cartItems = await _client.GetFromJsonAsync<List<CartVM>>("Carts", JsonOptions) ?? new();
            }
            catch { }

            var favoriteIds = favorites.Where(f => f.IsFavorite).Select(f => f.ProductId).ToHashSet();
            var cartDict = cartItems.GroupBy(c => c.ProductId).ToDictionary(g => g.Key, g => g.Sum(c => c.Quantity));

            foreach (var product in products)
            {
                product.IsFavorite = favoriteIds.Contains(product.Id);
                if (cartDict.TryGetValue(product.Id, out int qty))
                {
                    product.QuantityInCart = qty;
                }
            }

            return products;
        }

        public async Task<ProductDetailsVM?> GetProductDetailsAsync(int id)
        {
            ProductDetailsVM? product = null;

            try
            {
                product = await _client.GetFromJsonAsync<ProductDetailsVM>($"Products/{id}", JsonOptions);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ViewProductService] Error loading product {id}: {ex.Message}");
                return null;
            }

            if (product == null) return null;

            try
            {
                var favorites = await _client.GetFromJsonAsync<List<FavoriteVM>>("Favorites", JsonOptions);
                product.IsFavorite = favorites?.Any(f => f.ProductId == id && f.IsFavorite) ?? false;
            }
            catch { }

            try
            {
                var cartItems = await _client.GetFromJsonAsync<List<CartVM>>("Carts", JsonOptions);
                var itemInCart = cartItems?.FirstOrDefault(c => c.ProductId == id);
                if (itemInCart != null)
                {
                    product.QuantityInCart = itemInCart.Quantity;
                }
            }
            catch { }

            return product;
        }

        public async Task<CreateUpdateProductVM?> GetProductForEditAsync(int id)
        {
            try
            {
                return await _client.GetFromJsonAsync<CreateUpdateProductVM>($"Products/{id}", JsonOptions);
            }
            catch
            {
                return null;
            }
        }

        public async Task<SelectList> GetCategoriesSelectListAsync(object? selectedCategory = null)
        {
            try
            {
                var categories = await _client.GetFromJsonAsync<List<CategoryVM>>("Categories", JsonOptions)
                                 ?? new List<CategoryVM>();
                return new SelectList(categories, "Id", "Name", selectedCategory);
            }
            catch
            {
                return new SelectList(Enumerable.Empty<CategoryVM>(), "Id", "Name");
            }
        }

        public async Task<(bool Success, string Message)> CreateProductAsync(CreateUpdateProductVM model)
        {
            string? imagePath = model.ImgFile is { Length: > 0 }
                ? await SaveImageLocallyAsync(model.ImgFile)
                : null;

            using var content = BuildProductFormData(model, imagePath);
            var response = await _client.PostAsync("Products", content);

            if (response.IsSuccessStatusCode)
            {
                return (true, string.Empty);
            }

            if (!string.IsNullOrEmpty(imagePath))
                DeleteLocalImage(imagePath);

            var errorDetails = await response.Content.ReadAsStringAsync();
            return (false, $"API Error: {errorDetails}");
        }

        public async Task<(bool Success, string Message)> UpdateProductAsync(int id, CreateUpdateProductVM model)
        {
            string? updatedImagePath = model.Img;

            if (model.ImgFile is { Length: > 0 })
            {
                if (!string.IsNullOrEmpty(model.Img))
                    DeleteLocalImage(model.Img);

                updatedImagePath = await SaveImageLocallyAsync(model.ImgFile);
            }

            using var content = BuildProductFormData(model, updatedImagePath, includeId: true);
            var response = await _client.PutAsync($"Products/{id}", content);

            if (response.IsSuccessStatusCode)
            {
                return (true, string.Empty);
            }

            var errorDetails = await response.Content.ReadAsStringAsync();
            return (false, $"API Error: {errorDetails}");
        }

        public async Task<(bool Success, string Message)> DeleteProductAsync(int id)
        {
            var product = await _client.GetFromJsonAsync<ProductDetailsVM>($"Products/{id}", JsonOptions);
            var response = await _client.DeleteAsync($"Products/{id}");

            if (response.IsSuccessStatusCode)
            {
                if (product != null && !string.IsNullOrEmpty(product.Img))
                    DeleteLocalImage(product.Img);

                return (true, string.Empty);
            }

            var errorDetails = await response.Content.ReadAsStringAsync();
            return (false, $"API Error: {errorDetails}");
        }

        public async Task<string> SaveImageLocallyAsync(IFormFile file)
        {
            string folderPath = Path.Combine(_webHostEnvironment.WebRootPath, "images", "products");
            if (!Directory.Exists(folderPath))
                Directory.CreateDirectory(folderPath);

            string uniqueFileName = $"{Guid.NewGuid()}_{Path.GetFileName(file.FileName)}";
            string fullPath = Path.Combine(folderPath, uniqueFileName);

            await using var stream = new FileStream(fullPath, FileMode.Create);
            await file.CopyToAsync(stream);

            return $"/images/products/{uniqueFileName}";
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

        private static MultipartFormDataContent BuildProductFormData(CreateUpdateProductVM model, string? imagePath, bool includeId = false)
        {
            var content = new MultipartFormDataContent();
            if (includeId)
            {
                content.Add(new StringContent(model.Id.ToString()), "Id");
            }

            content.Add(new StringContent(model.Name ?? string.Empty), "Name");
            content.Add(new StringContent(model.Price.ToString(CultureInfo.InvariantCulture)), "Price");
            content.Add(new StringContent(model.Quantity.ToString()), "Quantity");
            content.Add(new StringContent(model.CategoryId.ToString()), "CategoryId");

            if (model.PowerType.HasValue)
            {
                content.Add(new StringContent(((int)model.PowerType.Value).ToString()), "PowerType");
            }

            content.Add(new StringContent(imagePath ?? string.Empty), "Img");
            return content;
        }
    }
}