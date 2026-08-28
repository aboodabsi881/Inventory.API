using Inventory.Web.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using System.Threading.Tasks;

namespace Inventory.Web.Controllers
{
    [Authorize]
    public class FavoriteController : Controller
    {
        private readonly IViewFavoriteService _favoriteService;
        private readonly IStringLocalizer<SharedResource> _localizer;

        public FavoriteController(
            IViewFavoriteService favoriteService,
            IStringLocalizer<SharedResource> localizer)
        {
            _favoriteService = favoriteService;
            _localizer = localizer;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var favorites = await _favoriteService.GetUserFavoritesAsync();
            return View(favorites);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Add(int productId)
        {
            if (productId <= 0)
                return BadRequest(new { icon = "error", message = _localizer["ProductNotFound"].Value });

            var result = await _favoriteService.ToggleFavoriteAsync(productId);
            if (result.Success)
            {
                return Ok(new
                {
                    icon = "success",
                    message = result.IsFavorite
                        ? _localizer["AddedToFavorite"].Value
                        : _localizer["RemovedFromFavorite"].Value,
                    isFavorite = result.IsFavorite,
                    productId
                });
            }

            return BadRequest(new { icon = "error", message = $"API Error: {result.Message}" });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (id <= 0)
                return BadRequest(new { icon = "error", message = _localizer["ProductNotFound"].Value });

            var result = await _favoriteService.DeleteFavoriteAsync(id);
            if (result.Success)
            {
                return Ok(new
                {
                    icon = "info",
                    message = _localizer["RemovedFromFavorite"].Value,
                    deletedId = id,
                    isFavorite = false
                });
            }

            return BadRequest(new { icon = "error", message = _localizer["ProductNotFound"].Value });
        }
    }
}