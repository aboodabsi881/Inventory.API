using Inventory.Web.Interfaces;
using Inventory.Web.ViewModels.Products;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using System.Threading.Tasks;

namespace Inventory.Web.Controllers
{
    public class ProductsController : Controller
    {
        private readonly IViewProductService _productService;
        private readonly IStringLocalizer<SharedResource> _localizer;

        public ProductsController(
            IViewProductService productService,
            IStringLocalizer<SharedResource> localizer)
        {
            _productService = productService;
            _localizer = localizer;
        }

        [AllowAnonymous]
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var products = await _productService.GetAllProductsAsync();
            return View(products);
        }

        [AllowAnonymous]
        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            if (id <= 0) return BadRequest("Invalid Product ID.");

            var product = await _productService.GetProductDetailsAsync(id);
            if (product == null) return NotFound();

            return View(product);
        }

        [HttpGet]
        [Authorize(Roles = "SuperAdmin, Admin")]
        public async Task<IActionResult> Create()
        {
            ViewBag.Categories = await _productService.GetCategoriesSelectListAsync();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "SuperAdmin, Admin")]
        public async Task<IActionResult> Create(CreateUpdateProductVM model)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Categories = await _productService.GetCategoriesSelectListAsync(model.CategoryId);
                return BadRequest(new { icon = "warning", message = _localizer["ValidationFailed"].Value });
            }

            var result = await _productService.CreateProductAsync(model);
            if (result.Success)
            {
                return Ok(new
                {
                    icon = "success",
                    message = _localizer["CreatedProducts"].Value,
                    redirectUrl = Url.Action("Index")
                });
            }

            return BadRequest(new { icon = "error", message = result.Message });
        }

        [HttpGet]
        [Authorize(Roles = "SuperAdmin, Admin")]
        public async Task<IActionResult> Edit(int id)
        {
            if (id <= 0) return BadRequest("Invalid Product ID.");

            var product = await _productService.GetProductForEditAsync(id);
            if (product == null) return NotFound();

            ViewBag.Categories = await _productService.GetCategoriesSelectListAsync(product.CategoryId);
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
                ViewBag.Categories = await _productService.GetCategoriesSelectListAsync(model.CategoryId);
                return BadRequest(new { icon = "warning", message = _localizer["ValidationFailed"].Value });
            }

            var result = await _productService.UpdateProductAsync(id, model);
            if (result.Success)
            {
                return Ok(new
                {
                    icon = "success",
                    message = _localizer["EditedProduct"].Value,
                    redirectUrl = Url.Action("Index")
                });
            }

            return BadRequest(new { icon = "error", message = result.Message });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "SuperAdmin, Admin")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (id <= 0) return BadRequest(new { icon = "error", message = "Invalid Product ID." });

            var result = await _productService.DeleteProductAsync(id);
            if (result.Success)
            {
                return Ok(new
                {
                    icon = "success",
                    message = _localizer["ProductDeleted"].Value,
                    deletedId = id,
                    redirectUrl = Url.Action("Index")
                });
            }

            return BadRequest(new { icon = "error", message = result.Message });
        }
    }
}