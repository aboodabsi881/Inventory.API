using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;
using Inventory.Core.DTOs;
using Inventory.Core.Interfaces;

namespace Inventory.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class FavoritesController : ControllerBase
    {
        private readonly IFavoriteService _favoriteService;

        public FavoritesController(IFavoriteService favoriteService)
        {
            _favoriteService = favoriteService;
        }

        // GET: api/favorites
        [HttpGet]
        public async Task<ActionResult<IReadOnlyList<FavoriteResponseDto>>> GetAllFavorites()
        {
            var favorites = await _favoriteService.GetAllFavoritesAsync();
            return Ok(favorites);
        }

        // GET: api/favorites/check/5
        [HttpGet("check/{productId:int}")]
        public async Task<ActionResult<bool>> IsProductFavorite(int productId)
        {
            var isFavorite = await _favoriteService.IsProductFavoriteAsync(productId);
            return Ok(new { productId, isFavorite });
        }

        // POST: api/favorites/toggle/5
        [HttpPost("toggle/{productId:int}")]
        public async Task<ActionResult<FavoriteResponseDto>> ToggleFavorite(int productId)
        {
            var favorite = await _favoriteService.ToggleFavoriteAsync(productId);
            if (favorite == null)
                return NotFound(new { message = $"Product with ID {productId} was not found." });

            return Ok(favorite);
        }

        // DELETE: api/favorites/5
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DeleteFavorite(int id)
        {
            var success = await _favoriteService.DeleteFavoriteAsync(id);
            if (!success)
                return NotFound(new { message = $"Favorite record with ID {id} was not found." });

            return NoContent();
        }
    }
}