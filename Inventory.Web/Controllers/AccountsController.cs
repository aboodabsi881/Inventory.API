using Inventory.Web.Interfaces;
using Inventory.Web.ViewModels.Accounts;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Inventory.Web.Controllers
{
    public class AccountsController : Controller
    {
        private readonly IViewAccountService _accountService;

        public AccountsController(IViewAccountService accountService)
        {
            _accountService = accountService;
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult AccessDenied() => View();

        [HttpGet]
        [Authorize(Roles = "SuperAdmin, Super Admin")]
        public async Task<IActionResult> Index()
        {
            var users = await _accountService.GetAllUsersAsync();
            return View(users);
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult Login()
        {
            if (User.Identity?.IsAuthenticated ?? false)
            {
                // Redirect already authenticated users based on their role
                if (User.IsInRole("User"))
                    return RedirectToAction("Index", "Category");

                return RedirectToAction("Index", "Home");
            }

            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginVM model)
        {
            if (!ModelState.IsValid)
                return BadRequest(new { icon = "error", message = "Invalid input details." });

            var result = await _accountService.LoginAsync(model);
            if (!result.Success || result.User == null)
                return BadRequest(new { icon = "error", message = result.Message });

            var principal = _accountService.CreateClaimsPrincipal(result.User);
            var authProperties = new AuthenticationProperties
            {
                IsPersistent = model.RememberMe,
                ExpiresUtc = DateTimeOffset.UtcNow.AddDays(7)
            };

            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal, authProperties);

            // Check if the user has the "User" role
            bool isStandardUser = principal.IsInRole("User")
                || string.Equals(result.User.RoleName, "User", StringComparison.OrdinalIgnoreCase);

            // Direct "User" role to Category/Index, others (e.g. Admin) to Home/Index
            string? targetUrl = isStandardUser
                ? Url.Action("Index", "Categories")
                : Url.Action("Index", "Home");

            return Ok(new
            {
                icon = "success",
                message = result.Message,
                redirectUrl = targetUrl
            });
        }

        [HttpGet]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction(nameof(Login));
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult Register() => View();

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterVM model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new
                {
                    errors = ModelState.ToDictionary(
                        kvp => kvp.Key,
                        kvp => kvp.Value?.Errors.Select(e => e.ErrorMessage).ToArray()
                    )
                });
            }

            var result = await _accountService.RegisterAsync(model);
            if (result.Success)
            {
                return Ok(new
                {
                    icon = "success",
                    message = result.Message,
                    redirectUrl = Url.Action("Login")
                });
            }

            return BadRequest(new { icon = "error", message = result.Message });
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> PersonalData(int id)
        {
            if (id <= 0) return BadRequest("Invalid User ID.");

            var vm = await _accountService.GetPersonalDataAsync(id);
            if (vm == null) return NotFound();

            return View(vm);
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PersonalData(PersonalDataVM model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new
                {
                    errors = ModelState.ToDictionary(
                        kvp => kvp.Key,
                        kvp => kvp.Value?.Errors.Select(e => e.ErrorMessage).ToArray()
                    )
                });
            }

            var result = await _accountService.UpdatePersonalDataAsync(model);
            if (result.Success)
            {
                return Ok(new
                {
                    icon = "success",
                    message = result.Message,
                    redirectUrl = Url.Action("Index", "Accounts")
                });
            }

            return BadRequest(new { icon = "error", message = result.Message });
        }

        [Authorize]
        [HttpGet]
        public IActionResult ChangePassword(int id)
        {
            if (id <= 0) return BadRequest("User ID is required.");
            return View(new ChangePasswordVM { Id = id.ToString() });
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(ChangePasswordVM model)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .Where(msg => !string.IsNullOrWhiteSpace(msg));

                return BadRequest(new { icon = "error", message = string.Join("<br/>", errors) });
            }

            var result = await _accountService.ChangePasswordAsync(model);
            if (result.Success)
            {
                return Ok(new
                {
                    icon = "success",
                    message = result.Message,
                    redirectUrl = Url.Action("Index", "Home")
                });
            }

            return BadRequest(new { icon = "error", message = result.Message });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "SuperAdmin, Super Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            if (id <= 0) return BadRequest(new { icon = "error", message = "Invalid User ID." });

            var result = await _accountService.DeleteUserAsync(id);
            if (result.Success)
            {
                return Ok(new
                {
                    icon = "info",
                    title = "Deleted!",
                    message = result.Message
                });
            }

            return BadRequest(new { icon = "error", message = result.Message });
        }
    }
}