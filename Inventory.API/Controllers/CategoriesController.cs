using Inventory.Core.DTOs;
using Inventory.Core.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Inventory.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CategoriesController : ControllerBase
    {
        private readonly ICategoryService _categoryService;

        public CategoriesController(ICategoryService categoryService)
        {
            _categoryService = categoryService;
        }

        // GET: api/categories
        [HttpGet]
        public async Task<ActionResult<IReadOnlyList<CategoryResponseDto>>> GetAllCategories()
        {
            var categories = await _categoryService.GetAllCategoriesAsync();
            return Ok(categories);
        }

        // GET: api/categories/index
        [HttpGet("index")]
        public async Task<ActionResult<CategoryIndexResponseDto>> GetCategoriesIndex()
        {
            var indexData = await _categoryService.GetCategoriesIndexAsync();
            return Ok(indexData);
        }

        // GET: api/categories/5
        [HttpGet("{id:int}")]
        public async Task<ActionResult<CategoryResponseDto>> GetCategoryById(int id)
        {
            var category = await _categoryService.GetCategoryByIdAsync(id);
            if (category == null)
                return NotFound(new { message = $"Category with ID {id} was not found." });

            return Ok(category);
        }

        // POST: api/categories
        [HttpPost]
        public async Task<ActionResult<CategoryResponseDto>> CreateCategory([FromForm] CategoryRequestDto model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var createdCategory = await _categoryService.CreateCategoryAsync(model);
            return CreatedAtAction(nameof(GetCategoryById), new { id = createdCategory.Id }, createdCategory);
        }

        // PUT: api/categories/5
        [HttpPut("{id:int}")]
        public async Task<ActionResult<CategoryResponseDto>> UpdateCategory(int id, [FromForm] CategoryRequestDto model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var updatedCategory = await _categoryService.UpdateCategoryAsync(id, model);
                return Ok(updatedCategory);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

        // DELETE: api/categories/5
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DeleteCategory(int id)
        {
            try
            {
                var success = await _categoryService.DeleteCategoryAsync(id);
                if (!success)
                    return BadRequest(new { message = "Failed to delete category." });

                return NoContent();
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }
    }
}