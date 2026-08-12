using Inventory.Web;
using Inventory.Web.ViewModels.Carts;
using Inventory.Web.ViewModels.Categories;
using Inventory.Web.ViewModels.Favorites;
using Inventory.Web.ViewModels.Products;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Localization;
using System.Globalization;
using System.Text.Json;

namespace Inventory.Web.Controllers
{
    public class ProductsController : Controller
    {
        private readonly HttpClient _client;
        private readonly IStringLocalizer<SharedResource> _localizer;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

        public ProductsController(
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
            var products = await _client.GetFromJsonAsync<List<ProductVM>>("Products", JsonOptions) ?? new List<ProductVM>();

            List<FavoriteVM> favorites = new();
            List<CartVM> cartItems = new();

            try
            {
                favorites = await _client.GetFromJsonAsync<List<FavoriteVM>>("Favorites", JsonOptions) ?? new List<FavoriteVM>();
                cartItems = await _client.GetFromJsonAsync<List<CartVM>>("Carts", JsonOptions) ?? new List<CartVM>();
            }
            catch
            {
            }

            var favoriteProductIds = favorites.Where(f => f.IsFavorite).Select(f => f.ProductId).ToHashSet();
            var cartDictionary = cartItems.ToDictionary(c => c.ProductId, c => c.Quantity);

            foreach (var product in products)
            {
                product.IsFavorite = favoriteProductIds.Contains(product.Id);
                if (cartDictionary.TryGetValue(product.Id, out int qty))
                {
                    product.QuantityInCart = qty;
                }
            }

            return View(products);
        }

        [AllowAnonymous]
        public async Task<IActionResult> Details(int id)
        {
            var product = await _client.GetFromJsonAsync<ProductDetailsVM>($"Products/{id}", JsonOptions);
            if (product == null) return NotFound();

            try
            {
                var favorites = await _client.GetFromJsonAsync<List<FavoriteVM>>("Favorites", JsonOptions) ?? new List<FavoriteVM>();
                var cartItems = await _client.GetFromJsonAsync<List<CartVM>>("Carts", JsonOptions) ?? new List<CartVM>();

                product.IsFavorite = favorites.Any(f => f.ProductId == id && f.IsFavorite);

                var cartItem = cartItems.FirstOrDefault(c => c.ProductId == id);
                if (cartItem != null)
                {
                    product.QuantityInCart = cartItem.Quantity;
                }
            }
            catch
            {
            }

            return View(product);
        }

        [Authorize(Roles = "SuperAdmin, Admin")]
        public async Task<IActionResult> Create()
        {
            await PopulateCategoriesDropdownAsync();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "SuperAdmin, Admin")]
        public async Task<IActionResult> Create(CreateUpdateProductVM model)
        {
            if (!ModelState.IsValid)
            {
                await PopulateCategoriesDropdownAsync(model.CategoryId);
                return BadRequest(new { icon = "warning", message = _localizer["ValidationFailed"].Value });
            }

            string? imagePath = model.ImgFile is { Length: > 0 } ? await SaveImageLocallyAsync(model.ImgFile) : null;

            using var content = BuildProductFormData(model, imagePath);
            var response = await _client.PostAsync("Products", content);

            if (response.IsSuccessStatusCode)
            {
                return Ok(new
                {
                    icon = "success",
                    message = _localizer["CreatedProducts"].Value,
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
            var product = await _client.GetFromJsonAsync<CreateUpdateProductVM>($"Products/{id}", JsonOptions);
            if (product == null) return NotFound();

            await PopulateCategoriesDropdownAsync(product.CategoryId);
            return View(product);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "SuperAdmin, Admin")]
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

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "SuperAdmin, Admin")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var product = await _client.GetFromJsonAsync<ProductDetailsVM>($"Products/{id}", JsonOptions);
            var response = await _client.DeleteAsync($"Products/{id}");

            if (response.IsSuccessStatusCode)
            {
                if (product != null && !string.IsNullOrEmpty(product.Img))
                    DeleteLocalImage(product.Img);

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

        private async Task PopulateCategoriesDropdownAsync(object? selectedCategory = null)
        {
            var categories = await _client.GetFromJsonAsync<List<CategoryVM>>("Categories", JsonOptions) ?? new List<CategoryVM>();
            ViewBag.Categories = new SelectList(categories, "Id", "Name", selectedCategory);
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
            content.Add(new StringContent(model.CategoryId.ToString()), "CategoryId");

            if (model.PowerType.HasValue)
            {
                content.Add(new StringContent(((int)model.PowerType.Value).ToString()), "PowerType");
            }

            content.Add(new StringContent(imagePath ?? string.Empty), "Img");
            return content;
        }

        private async Task<string> SaveImageLocallyAsync(IFormFile file)
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