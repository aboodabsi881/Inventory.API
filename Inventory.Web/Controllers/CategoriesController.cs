using Inventory.ViewModel.Categories;
using Inventory.Web.Resources;
using Inventory.Web.ViewModels.Carts;
using Inventory.Web.ViewModels.Favorites;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using System.Net.Http.Json;
using System.Text.Json;

namespace Inventory.Web.Controllers
{
    public class CategoriesController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IStringLocalizer<SharedResource> _localizer;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public CategoriesController(
            IHttpClientFactory httpClientFactory,
            IStringLocalizer<SharedResource> localizer,
            IWebHostEnvironment webHostEnvironment)
        {
            _httpClientFactory = httpClientFactory;
            _localizer = localizer;
            _webHostEnvironment = webHostEnvironment;
        }

        // GET: Categories
        // Retrieves and displays all categories from the API.
        public async Task<IActionResult> Index()
        {
            var client = _httpClientFactory.CreateClient("InventoryAPI");
            var categories = await client.GetFromJsonAsync<List<CategoryVM>>("Categories");

            return View(categories ?? new List<CategoryVM>());
        }

        // GET: Categories/Details/5
        // Retrieves category details along with associated products from the API.
        // GET: Categories/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var client = _httpClientFactory.CreateClient("InventoryAPI");
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

            // 1️⃣ جلب تفاصيل التصنيف مع منتجاته
            var category = await client.GetFromJsonAsync<CategoryDetailsVM>($"Categories/{id}", options);

            if (category == null)
                return NotFound();

            // 2️⃣ جلب قائمة المفضلات الحالية من الـ API
            var favorites = await client.GetFromJsonAsync<List<FavoriteVM>>("Favorites", options) ?? new List<FavoriteVM>();
            var favoriteProductIds = favorites.Where(f => f.IsFavorite).Select(f => f.ProductId).ToHashSet();

            var carts = await client.GetFromJsonAsync<List<CartVM>>("Carts", options) ?? new List<CartVM>();
            var cartProductIds = carts.Select(f => f.ProductId).ToHashSet();

            // 3️⃣ تحديث حالة IsFavorite لكل منتج داخل هذا التصنيف
            if (category.ProductsVM != null)
            {
                foreach (var product in category.ProductsVM)
                {
                    product.IsFavorite = favoriteProductIds.Contains(product.Id);
                }
            }

            return View(category);
        }

        // GET: Categories/Create
        // Renders the form to create a new category.
        public IActionResult Create()
        {
            return View();
        }

        // POST: Categories/Create
        // Saves image locally in wwwroot/images/categories and posts category data to the API.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateUpdateCategoryVM model)
        {
            if (!ModelState.IsValid)
                return BadRequest(new { icon = "warning", message = _localizer["ValidationFailed"].Value });

            string? imagePath = null;

            // Handle image upload and save it locally inside Inventory.Web/wwwroot
            if (model.ImgFile != null && model.ImgFile.Length > 0)
            {
                imagePath = await SaveImageLocallyAsync(model.ImgFile);
            }

            var client = _httpClientFactory.CreateClient("InventoryAPI");

            using var content = new MultipartFormDataContent();
            content.Add(new StringContent(model.Name ?? string.Empty), "Name");

            // Pass saved image relative path to API (or empty string if no image uploaded)
            content.Add(new StringContent(imagePath ?? string.Empty), "Img");

            var response = await client.PostAsync("Categories", content);

            if (response.IsSuccessStatusCode)
            {
                return Ok(new
                {
                    icon = "success",
                    message = _localizer["CreatedCategory"].Value,
                    redirectUrl = Url.Action("Index")
                });
            }

            // If API call fails, remove uploaded image to avoid unused file accumulation
            if (!string.IsNullOrEmpty(imagePath))
            {
                DeleteLocalImage(imagePath);
            }

            var errorDetails = await response.Content.ReadAsStringAsync();
            return BadRequest(new { icon = "error", message = $"API Error: {errorDetails}" });
        }

        // GET: Categories/Edit/5
        // Retrieves existing category details to populate the edit form.
        public async Task<IActionResult> Edit(int id)
        {
            var client = _httpClientFactory.CreateClient("InventoryAPI");
            var category = await client.GetFromJsonAsync<CreateUpdateCategoryVM>($"Categories/{id}");

            if (category == null)
                return NotFound();

            return View(category);
        }

        // POST: Categories/Edit/5
        // Updates category data and replaces old local image file if a new file is uploaded.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, CreateUpdateCategoryVM model)
        {
            if (id != model.Id)
                return BadRequest(new { icon = "error", message = _localizer["CategoryNotFound"].Value });

            if (!ModelState.IsValid)
                return BadRequest(new { icon = "warning", message = _localizer["ValidationFailed"].Value });

            string? updatedImagePath = model.Img;

            // Check if a new image file was uploaded during edit
            if (model.ImgFile != null && model.ImgFile.Length > 0)
            {
                // Delete existing old image file from wwwroot if present
                if (!string.IsNullOrEmpty(model.Img))
                {
                    DeleteLocalImage(model.Img);
                }

                // Save new image file locally in wwwroot
                updatedImagePath = await SaveImageLocallyAsync(model.ImgFile);
            }

            var client = _httpClientFactory.CreateClient("InventoryAPI");

            using var content = new MultipartFormDataContent();
            content.Add(new StringContent(model.Id.ToString()), "Id");
            content.Add(new StringContent(model.Name ?? string.Empty), "Name");
            content.Add(new StringContent(updatedImagePath ?? string.Empty), "Img");

            var response = await client.PutAsync($"Categories/{id}", content);

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

        // POST: Categories/Delete/5
        // Deletes category from API and removes its associated local image file from wwwroot.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var client = _httpClientFactory.CreateClient("InventoryAPI");

            // Retrieve category details to acquire the image path before deletion
            var category = await client.GetFromJsonAsync<CategoryDetailsVM>($"Categories/{id}");

            var response = await client.DeleteAsync($"Categories/{id}");

            if (response.IsSuccessStatusCode)
            {
                // Remove corresponding local image from wwwroot upon successful deletion
                if (category != null && !string.IsNullOrEmpty(category.Img))
                {
                    DeleteLocalImage(category.Img);
                }

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

        #region Helper Methods for Local Image Storage

        // Saves uploaded file inside Inventory.Web/wwwroot/images/categories/ and returns the relative path.
        private async Task<string> SaveImageLocallyAsync(IFormFile file)
        {
            string folderPath = Path.Combine(_webHostEnvironment.WebRootPath, "images", "categories");

            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }

            string uniqueFileName = $"{Guid.NewGuid()}_{Path.GetFileName(file.FileName)}";
            string fullPath = Path.Combine(folderPath, uniqueFileName);

            using (var stream = new FileStream(fullPath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            return $"/images/categories/{uniqueFileName}";
        }

        // Removes a physical file from wwwroot if it exists.
        private void DeleteLocalImage(string relativePath)
        {
            if (string.IsNullOrWhiteSpace(relativePath)) return;

            string fullPath = Path.Combine(_webHostEnvironment.WebRootPath, relativePath.TrimStart('/'));

            if (System.IO.File.Exists(fullPath))
            {
                System.IO.File.Delete(fullPath);
            }
        }

        #endregion
    }
}