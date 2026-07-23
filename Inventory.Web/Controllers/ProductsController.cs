using Inventory.ViewModel.Categories;
using Inventory.ViewModel.Products;
using Inventory.Web.Resources;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Localization;
using System.Globalization;
using System.Net.Http.Json;

namespace Inventory.Web.Controllers
{
    public class ProductsController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IStringLocalizer<SharedResource> _localizer;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public ProductsController(
            IHttpClientFactory httpClientFactory,
            IStringLocalizer<SharedResource> localizer,
            IWebHostEnvironment webHostEnvironment)
        {
            _httpClientFactory = httpClientFactory;
            _localizer = localizer;
            _webHostEnvironment = webHostEnvironment;
        }

        // GET: Products
        // Retrieves and displays all products from the API.
        public async Task<IActionResult> Index()
        {
            var client = _httpClientFactory.CreateClient("InventoryAPI");
            var products = await client.GetFromJsonAsync<List<ProductVM>>("Products");

            return View(products ?? new List<ProductVM>());
        }

        // GET: Products/Details/5
        // Retrieves product details.
        public async Task<IActionResult> Details(int id)
        {
            var client = _httpClientFactory.CreateClient("InventoryAPI");

            var options = new System.Text.Json.JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            // 💡 تمرير options يضمن قراءة categoryName بنجاح بغض النظر عن حالة الأحرف
            var product = await client.GetFromJsonAsync<ProductDetailsVM>($"Products/{id}", options);

            if (product == null)
                return NotFound();

            return View(product);
        }

        // GET: Products/Create
        // Renders the form to create a new product and loads categories for dropdown.
        public async Task<IActionResult> Create()
        {
            await PopulateCategoriesDropdownAsync();
            return View();
        }

        // POST: Products/Create
        // Saves image locally in wwwroot/images/products and posts product data to the API.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateUpdateProductVM model)
        {
            if (!ModelState.IsValid)
            {
                await PopulateCategoriesDropdownAsync(model.CategoryId);
                return BadRequest(new { icon = "warning", message = _localizer["ValidationFailed"].Value });
            }

            string? imagePath = null;

            if (model.ImgFile != null && model.ImgFile.Length > 0)
            {
                imagePath = await SaveImageLocallyAsync(model.ImgFile);
            }

            var client = _httpClientFactory.CreateClient("InventoryAPI");

            using var content = new MultipartFormDataContent();
            content.Add(new StringContent(model.Name ?? string.Empty), "Name");

            content.Add(new StringContent(model.Price.ToString(CultureInfo.InvariantCulture)), "Price");
            content.Add(new StringContent(model.CategoryId.ToString()), "CategoryId");

            if (model.PowerType.HasValue)
            {
                content.Add(new StringContent(model.PowerType.Value.ToString()), "PowerType");
            }

            content.Add(new StringContent(imagePath ?? string.Empty), "Img");
            var response = await client.PostAsync("Products", content);

            if (response.IsSuccessStatusCode)
            {
                return Ok(new
                {
                    icon = "success",
                    message = _localizer["CreatedProducts"].Value,
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

        // GET: Products/Edit/5
        // Retrieves existing Product details and loads categories dropdown to populate the edit form.
        public async Task<IActionResult> Edit(int id)
        {
            var client = _httpClientFactory.CreateClient("InventoryAPI");
            var product = await client.GetFromJsonAsync<CreateUpdateProductVM>($"Products/{id}");

            if (product == null)
                return NotFound();

            await PopulateCategoriesDropdownAsync(product.CategoryId);
            return View(product);
        }

        // POST: Products/Edit/5
        // Updates product data and replaces old local image file if a new file is uploaded.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, CreateUpdateProductVM model)
        {
            if (id != model.Id)
                return BadRequest(new { icon = "error", message = _localizer["ProductNotFound"].Value });

            if (!ModelState.IsValid)
            {
                await PopulateCategoriesDropdownAsync(model.CategoryId);
                return BadRequest(new { icon = "warning", message = _localizer["ValidationFailed"].Value });
            }

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
            content.Add(new StringContent(model.Price.ToString(CultureInfo.InvariantCulture)), "Price");
            content.Add(new StringContent(model.CategoryId.ToString()), "CategoryId");

            // FIX: Bind PowerType enum if populated
            if (model.PowerType.HasValue)
            {
                content.Add(new StringContent(((int)model.PowerType.Value).ToString()), "PowerType");
            }

            content.Add(new StringContent(updatedImagePath ?? string.Empty), "Img");
            var response = await client.PutAsync($"Products/{id}", content);

            if (response.IsSuccessStatusCode)
            {
                return Ok(new
                {
                    icon = "success",
                    message = _localizer["EditedProduct"].Value,
                    redirectUrl = Url.Action("Index")
                });
            }

            var errorDetails = await response.Content.ReadAsStringAsync();
            return BadRequest(new { icon = "error", message = $"API Error: {errorDetails}" });
        }

        // POST: Products/Delete/5
        // Deletes product from API and removes its associated local image file from wwwroot.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var client = _httpClientFactory.CreateClient("InventoryAPI");

            // Retrieve product details to acquire the image path before deletion
            var product = await client.GetFromJsonAsync<ProductDetailsVM>($"Products/{id}");

            var response = await client.DeleteAsync($"Products/{id}");

            if (response.IsSuccessStatusCode)
            {
                // Remove corresponding local image from wwwroot upon successful deletion
                if (product != null && !string.IsNullOrEmpty(product.Img))
                {
                    DeleteLocalImage(product.Img);
                }

                return Ok(new
                {
                    icon = "success",
                    message = _localizer["ProductDeleted"].Value,
                    deletedId = id,
                    redirectUrl = Url.Action("Index")
                });
            }

            var errorDetails = await response.Content.ReadAsStringAsync();
            return BadRequest(new { icon = "error", message = $"API Error: {errorDetails}" });
        }

        #region Helper Methods

        // ✅ تصحيح: جلب التصنيفات الحقيقية من API
        private async Task PopulateCategoriesDropdownAsync(object? selectedCategory = null)
        {
            var client = _httpClientFactory.CreateClient("InventoryAPI");
            var categories = await client.GetFromJsonAsync<List<CategoryVM>>("Categories") ?? new List<CategoryVM>();
            ViewBag.Categories = new SelectList(categories, "Id", "Name", selectedCategory);
        }

        // Saves uploaded file inside Inventory.Web/wwwroot/images/products/ and returns the relative path.
        private async Task<string> SaveImageLocallyAsync(IFormFile file)
        {
            string folderPath = Path.Combine(_webHostEnvironment.WebRootPath, "images", "products");

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

            return $"/images/products/{uniqueFileName}";
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