    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Inventory.Core.DTOs;
    using Inventory.Core.Interfaces;

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

            // GET: api/categories/5
            [HttpGet("{id:int}")]
            public async Task<ActionResult<CategoryResponseDto>> GetCategoryById(int id)
            {
                try
                {
                    var category = await _categoryService.GetCategoryByIdAsync(id);
                    return Ok(category);
                }
                catch (KeyNotFoundException ex)
                {
                    return NotFound(new { message = ex.Message });
                }
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
        // PUT: api/categories/5
        [HttpPut("{id:int}")]
        public async Task<ActionResult<CategoryResponseDto>> UpdateCategory(int id, [FromForm] CategoryRequestDto model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                // إرسال الـ model مباشرة بدون ملفات منفصلة
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