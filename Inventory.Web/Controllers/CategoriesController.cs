using Inventory.Web.Resources;
using Inventory.Web.ViewModels.Carts;
using Inventory.Web.ViewModels.Categories;
using Inventory.Web.ViewModels.Favorites;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using System.Text.Json;

namespace Inventory.Web.Controllers
{
    public class CategoriesController : Controller
    {
        private readonly HttpClient _client;
        private readonly IStringLocalizer<SharedResource> _localizer;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

        public CategoriesController(
            IHttpClientFactory httpClientFactory,
            IStringLocalizer<SharedResource> localizer,
            IWebHostEnvironment webHostEnvironment)
        {
            _client = httpClientFactory.CreateClient("InventoryAPI");
            _localizer = localizer;
            _webHostEnvironment = webHostEnvironment;
        }

        [AllowAnonymous]
        public async Task<IActionResult> Index()
        {
            var categories = await _client.GetFromJsonAsync<List<CategoryVM>>("Categories", JsonOptions) ?? new List<CategoryVM>();
            return View(categories);
        }

        [AllowAnonymous]
        public async Task<IActionResult> Details(int id)
        {
            var category = await _client.GetFromJsonAsync<CategoryDetailsVM>($"Categories/{id}", JsonOptions);
            if (category == null)
            {
                return NotFound();
            } 

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

            return View(category);
        }

        [Authorize(Roles = "SuperAdmin, Admin")]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "SuperAdmin, Admin")]
        public async Task<IActionResult> Create(CreateUpdateCategoryVM model)
        {
            if (!ModelState.IsValid)
                return BadRequest(new { icon = "warning", message = _localizer["ValidationFailed"].Value });

            string? imagePath = model.ImgFile is { Length: > 0 } ? await SaveImageLocallyAsync(model.ImgFile) : null;

            using var content = new MultipartFormDataContent
            {
                { new StringContent(model.Name ?? string.Empty), "Name" },
                { new StringContent(imagePath ?? string.Empty), "Img" }
            };

            var response = await _client.PostAsync("Categories", content);
            if (response.IsSuccessStatusCode)
            {
                return Ok(new
                {
                    icon = "success",
                    message = _localizer["CreatedCategory"].Value,
                    redirectUrl = Url.Action("Index")
                });
            }

            if (!string.IsNullOrEmpty(imagePath))
                DeleteLocalImage(imagePath);

            var errorDetails = await response.Content.ReadAsStringAsync();
            return BadRequest(new { icon = "error", message = $"API Error: {errorDetails}" });
        }

        [Authorize(Roles = "SuperAdmin, Admin")]
        public async Task<IActionResult> Edit(int id)
        {
            var category = await _client.GetFromJsonAsync<CreateUpdateCategoryVM>($"Categories/{id}", JsonOptions);
            if (category == null) return NotFound();

            return View(category);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "SuperAdmin, Admin")]
        public async Task<IActionResult> Edit(int id, CreateUpdateCategoryVM model)
        {
            if (id != model.Id)
                return BadRequest(new { icon = "error", message = _localizer["CategoryNotFound"].Value });

            if (!ModelState.IsValid)
                return BadRequest(new { icon = "warning", message = _localizer["ValidationFailed"].Value });

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
                return Ok(new
                {
                    icon = "success",
                    message = _localizer["EditedCategory"].Value,
                    redirectUrl = Url.Action("Index")
                });
            }

            var errorDetails = await response.Content.ReadAsStringAsync();
            return BadRequest(new { icon = "error", message = $"API Error: {errorDetails}" });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "SuperAdmin, Admin")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var category = await _client.GetFromJsonAsync<CategoryDetailsVM>($"Categories/{id}", JsonOptions);
            var response = await _client.DeleteAsync($"Categories/{id}");

            if (response.IsSuccessStatusCode)
            {
                if (category != null && !string.IsNullOrEmpty(category.Img))
                    DeleteLocalImage(category.Img);

                return Ok(new
                {
                    icon = "success",
                    message = _localizer["CategoryDeleted"].Value,
                    deletedId = id,
                    redirectUrl = Url.Action("Index")
                });
            }

            return BadRequest(new { icon = "error", message = "Failed to delete category via API." });
        }

        private async Task<string> SaveImageLocallyAsync(IFormFile file)
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

        private void DeleteLocalImage(string relativePath)
        {
            if (string.IsNullOrWhiteSpace(relativePath)) return;

            string fullPath = Path.Combine(_webHostEnvironment.WebRootPath, relativePath.TrimStart('/'));
            if (System.IO.File.Exists(fullPath))
            {
                System.IO.File.Delete(fullPath);
            }
        }
    }
}