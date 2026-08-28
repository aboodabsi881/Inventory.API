using Inventory.Core.DTOs;
using Inventory.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

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

        [HttpGet]
        public async Task<ActionResult<IReadOnlyList<CartResponseDto>>> GetCart()
        {
            try
            {
                var cartItems = await _cartService.GetCartAsync();
                return Ok(cartItems);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
        }

        [HttpGet("total")]
        public async Task<ActionResult<decimal>> GetCartTotal()
        {
            try
            {
                var total = await _cartService.GetCartTotalAsync();
                return Ok(new { total });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
        }

        [HttpPost("items")]
        public async Task<IActionResult> AddOrUpdateItem([FromQuery] int productId, [FromQuery] int change = 1)
        {
            try
            {
                var result = await _cartService.AddOrUpdateItemAsync(productId, change);

                if (result == null)
                {
                    return Ok(new
                    {
                        message = "Item quantity reduced to zero and removed from cart.",
                        quantity = 0,
                        removed = true
                    });
                }

                return Ok(result);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        [HttpDelete("{cartId:int}")]
        public async Task<IActionResult> RemoveItem(int cartId)
        {
            try
            {
                var success = await _cartService.RemoveItemAsync(cartId);
                if (!success)
                    return NotFound(new { message = $"Cart item with ID {cartId} was not found." });

                return NoContent();
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
        }
    }
}