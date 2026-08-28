using Inventory.Web.Interfaces;
using Inventory.Web.ViewModels.Carts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using System.Threading.Tasks;

namespace Inventory.Web.Controllers
{
    [Authorize]
    public class CartController : Controller
    {
        private readonly IViewCartService _cartService;
        private readonly IStringLocalizer<SharedResource> _localizer;

        public CartController(IViewCartService cartService, IStringLocalizer<SharedResource> localizer)
        {
            _cartService = cartService;
            _localizer = localizer;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var cartItems = await _cartService.GetCartItemsAsync();
            ViewBag.GrandTotal = await _cartService.GetSafeCartTotalAsync();
            return View(cartItems);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddOrUpdate(int productId, string? actionType, int change = 1)
        {
            if (productId <= 0)
                return BadRequest(new { icon = "error", message = _localizer["Invalid Product ID."].Value });

            var result = await _cartService.AddOrUpdateItemAsync(productId, actionType, change);
            if (!result.Success)
            {
                return BadRequest(new
                {
                    icon = "error",
                    message = !string.IsNullOrWhiteSpace(result.ErrorMessage)
                        ? result.ErrorMessage
                        : _localizer["Failed to update cart."].Value
                });
            }

            return Ok(new
            {
                icon = result.Removed ? "info" : "success",
                message = result.Removed
                    ? _localizer["Item removed from cart."].Value
                    : _localizer["Cart updated successfully."].Value,
                item = result.Item,
                quantity = result.Quantity,
                grandTotal = result.GrandTotal,
                removed = result.Removed
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Remove(int id)
        {
            if (id <= 0)
                return BadRequest(new { icon = "error", message = _localizer["Invalid Cart ID."].Value });

            var result = await _cartService.RemoveItemAsync(id);
            if (!result.Success)
            {
                return BadRequest(new
                {
                    icon = "error",
                    message = !string.IsNullOrWhiteSpace(result.ErrorMessage)
                        ? result.ErrorMessage
                        : _localizer["Failed to remove item."].Value
                });
            }

            return Ok(new
            {
                icon = "info",
                message = _localizer["Item removed from cart."].Value,
                cartId = id,
                grandTotal = result.GrandTotal
            });
        }
    }
}