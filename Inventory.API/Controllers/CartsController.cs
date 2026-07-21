using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;
using Inventory.Core.DTOs;
using Inventory.Core.Interfaces;

namespace Inventory.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CartsController : ControllerBase
    {
        private readonly ICartService _cartService;

        public CartsController(ICartService cartService)
        {
            _cartService = cartService;
        }

        // GET: api/carts
        [HttpGet]
        public async Task<ActionResult<IReadOnlyList<CartResponseDto>>> GetCart()
        {
            var cartItems = await _cartService.GetCartAsync();
            return Ok(cartItems);
        }

        // GET: api/carts/total
        [HttpGet("total")]
        public async Task<ActionResult<decimal>> GetCartTotal()
        {
            var total = await _cartService.GetCartTotalAsync();
            return Ok(new { total });
        }

        // POST: api/carts/items?productId=5&change=1
        [HttpPost("items")]
        public async Task<ActionResult<CartResponseDto>> AddOrUpdateItem([FromQuery] int productId, [FromQuery] int change = 1)
        {
            try
            {
                var result = await _cartService.AddOrUpdateItemAsync(productId, change);
                if (result == null)
                    return Ok(new { message = "Item quantity reduced to zero and removed from cart." });

                return Ok(result);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

        // DELETE: api/carts/5
        [HttpDelete("{cartId:int}")]
        public async Task<IActionResult> RemoveItem(int cartId)
        {
            var success = await _cartService.RemoveItemAsync(cartId);
            if (!success)
                return NotFound(new { message = $"Cart item with ID {cartId} was not found." });

            return NoContent();
        }
    }
}