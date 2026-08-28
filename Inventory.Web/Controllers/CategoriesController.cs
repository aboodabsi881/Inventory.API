using Inventory.Web.Interfaces;
using Inventory.Web.ViewModels.Categories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using System.Threading.Tasks;

namespace Inventory.Web.Controllers
{
    public class CategoriesController : Controller
    {
        private readonly IViewCategoryService _categoryService;
        private readonly IStringLocalizer<SharedResource> _localizer;

        public CategoriesController(
            IViewCategoryService categoryService,
            IStringLocalizer<SharedResource> localizer)
        {
            _categoryService = categoryService;
            _localizer = localizer;
        }

        [AllowAnonymous]
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var categories = await _categoryService.GetAllCategoriesAsync();
            return View(categories);
        }

        [AllowAnonymous]
        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            if (id <= 0) return BadRequest("Invalid Category ID.");

            var category = await _categoryService.GetCategoryDetailsAsync(id);
            if (category == null) return NotFound();

            return View(category);
        }

        [HttpGet]
        [Authorize(Roles = "SuperAdmin, Admin")]
        public IActionResult Create() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "SuperAdmin, Admin")]
        public async Task<IActionResult> Create(CreateUpdateCategoryVM model)
        {
            if (!ModelState.IsValid)
                return BadRequest(new { icon = "warning", message = _localizer["ValidationFailed"].Value });

            var result = await _categoryService.CreateCategoryAsync(model);
            if (result.Success)
            {
                return Ok(new
                {
                    icon = "success",
                    message = _localizer["CreatedCategory"].Value,
                    redirectUrl = Url.Action("Index")
                });
            }

            return BadRequest(new { icon = "error", message = result.Message });
        }

        [HttpGet]
        [Authorize(Roles = "SuperAdmin, Admin")]
        public async Task<IActionResult> Edit(int id)
        {
            if (id <= 0) return BadRequest("Invalid Category ID.");

            var category = await _categoryService.GetCategoryForEditAsync(id);
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

            var result = await _categoryService.UpdateCategoryAsync(id, model);
            if (result.Success)
            {
                return Ok(new
                {
                    icon = "success",
                    message = _localizer["EditedCategory"].Value,
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
            if (id <= 0) return BadRequest(new { icon = "error", message = "Invalid Category ID." });

            var result = await _categoryService.DeleteCategoryAsync(id);
            if (result.Success)
            {
                return Ok(new
                {
                    icon = "info",
                    message = _localizer["CategoryDeleted"].Value,
                    deletedId = id,
                    redirectUrl = Url.Action("Index")
                });
            }

            return BadRequest(new { icon = "error", message = result.Message });
        }
    }
}