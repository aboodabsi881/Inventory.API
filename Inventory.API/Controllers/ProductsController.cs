using Inventory.Core.DTOs;
using Inventory.Core.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Inventory.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProductsController : ControllerBase
    {
        private readonly IProductService _productService;

        public ProductsController(IProductService productService)
        {
            _productService = productService;
        }

        // GET: api/products
        [HttpGet]
        public async Task<ActionResult<IReadOnlyList<ProductResponseDto>>> GetAllProducts()
        {
            // 💡 التعديل هنا: استخدام الاسم الجديد للميثود في السيرفيس
            var products = await _productService.GetAllProductsDtoAsync();
            return Ok(products);
        }

        // GET: api/products/5
        [HttpGet("{id:int}")]
        public async Task<ActionResult<ProductResponseDto>> GetProductById(int id)
        {
            // 💡 التعديل هنا: استخدام الاسم الجديد للميثود في السيرفيس
            var product = await _productService.GetProductDtoByIdAsync(id);

            if (product == null)
                return NotFound(new { message = $"Product with ID {id} was not found." });

            return Ok(product);
        }

        // POST: api/products
        [HttpPost]
        public async Task<ActionResult<ProductResponseDto>> CreateProduct([FromForm] ProductRequestDto model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var createdProduct = await _productService.CreateProductAsync(model);
            return CreatedAtAction(nameof(GetProductById), new { id = createdProduct.Id }, createdProduct);
        }

        // PUT: api/products/5
        [HttpPut("{id:int}")]
        public async Task<ActionResult<ProductResponseDto>> UpdateProduct(int id, [FromForm] ProductRequestDto model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var updatedProduct = await _productService.UpdateProductAsync(id, model);
                return Ok(updatedProduct);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

        // DELETE: api/products/5
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DeleteProduct(int id)
        {
            try
            {
                var success = await _productService.DeleteProductAsync(id);
                if (!success)
                    return BadRequest(new { message = "Failed to delete product." });

                return NoContent();
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }
    }
}