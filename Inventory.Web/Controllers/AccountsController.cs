using Inventory.Web.ViewModels.Accounts;
using Inventory.Web.ViewModels.Roles;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Security.Claims;
using System.Text.Json;

namespace Inventory.Web.Controllers
{
    public class AccountsController : Controller
    {
        private readonly HttpClient _client;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

        public AccountsController(IHttpClientFactory httpClientFactory, IWebHostEnvironment webHostEnvironment)
        {
            _client = httpClientFactory.CreateClient("InventoryAPI");
            _webHostEnvironment = webHostEnvironment;
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult AccessDenied()
        {
            return View();
        }

        [HttpGet]
        [Authorize(Roles = "SuperAdmin, Super Admin")]
        public async Task<IActionResult> Index()
        {
            var users = await _client.GetFromJsonAsync<List<UsersVM>>("Accounts/users", JsonOptions) ?? new List<UsersVM>();
            return View(users);
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult Login()
        {
            if (User.Identity?.IsAuthenticated ?? false)
            {
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
                return BadRequest(new { message = "Invalid input details." });

            var response = await _client.PostAsJsonAsync("Accounts/login", model);
            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                return BadRequest(new { message = !string.IsNullOrWhiteSpace(errorContent) ? errorContent : "Invalid username or password." });
            }

            var userObj = await response.Content.ReadFromJsonAsync<UsersVM>(JsonOptions);
            if (userObj == null)
                return BadRequest(new { message = "Invalid user response." });

            var principal = CreateClaimsPrincipal(userObj);
            var authProperties = new AuthenticationProperties
            {
                IsPersistent = model.RememberMe,
                ExpiresUtc = DateTimeOffset.UtcNow.AddDays(7)
            };

            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal, authProperties);

            return Ok(new
            {
                icon = "success",
                message = "Login successful!",
                redirectUrl = Url.Action("Index", "Accounts")
            });
        }

        [HttpGet]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            Response.Cookies.Delete("UserAvatar");
            Response.Cookies.Delete("UserName");
            Response.Cookies.Delete("UserRole");

            return RedirectToAction(nameof(Login));
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterVM model)
        {
            if (!ModelState.IsValid)
                return View(model);

            string? imagePath = model.ImgFile is { Length: > 0 } ? await SaveUserImageAsync(model.ImgFile) : null;

            var registerPayload = new
            {
                model.UserName,
                model.Email,
                model.Password,
                model.NameEn,
                model.NameAr,
                Img = !string.IsNullOrEmpty(imagePath) ? imagePath : "/images/Portrait_Placeholder.png",
                RoleName = string.IsNullOrEmpty(model.RoleName) ? "User" : model.RoleName
            };

            var response = await _client.PostAsJsonAsync("Accounts/register", registerPayload);
            if (response.IsSuccessStatusCode)
            {
                return Ok(new { icon = "success", message = "User registered successfully!", redirectUrl = Url.Action("Login") });
            }

            var errorContent = await response.Content.ReadAsStringAsync();
            return BadRequest(new { message = $"Registration failed: {errorContent}" });
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> PersonalData(int id)
        {
            if (id <= 0) return BadRequest("Invalid User ID.");

            var response = await _client.GetAsync($"Accounts/personal-data/{id}");
            if (!response.IsSuccessStatusCode) return NotFound();

            var vm = await response.Content.ReadFromJsonAsync<PersonalDataVM>(JsonOptions);
            if (vm == null) return NotFound();

            var rolesList = await _client.GetFromJsonAsync<List<RoleVM>>("Roles", JsonOptions) ?? new List<RoleVM>();
            vm.RoleLookup = new SelectList(rolesList.Select(r => r.Name), vm.RoleName);

            return View(vm);
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PersonalData(PersonalDataVM model)
        {
            if (!ModelState.IsValid)
            {
                var rolesList = await _client.GetFromJsonAsync<List<RoleVM>>("Roles", JsonOptions) ?? new List<RoleVM>();
                model.RoleLookup = new SelectList(rolesList.Select(r => r.Name), model.RoleName);
                return View(model);
            }

            if (model.ImgFile is { Length: > 0 })
            {
                model.Img = await SaveUserImageAsync(model.ImgFile);
            }
            else if (string.IsNullOrWhiteSpace(model.Img))
            {
                model.Img = "/images/Portrait_Placeholder.png";
            }

            using var formData = BuildPersonalDataFormData(model);
            var response = await _client.PutAsync($"Accounts/personal-data/{model.Id}", formData);

            if (response.IsSuccessStatusCode)
            {
                return Ok(new { icon = "success", message = "Profile updated successfully!", redirectUrl = Url.Action("Index", "Accounts") });
            }

            var errorContent = await response.Content.ReadAsStringAsync();
            return BadRequest(new { icon = "error", message = !string.IsNullOrEmpty(errorContent) ? errorContent : "Failed to update profile." });
        }

        [Authorize]
        [HttpGet]
        public IActionResult ChangePassword(int id)
        {
            if (id <= 0)
            {
                return BadRequest("User ID is required.");
            }
            return View(new ChangePasswordVM { Id = id.ToString() });
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(ChangePasswordVM model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var payload = new { CurrentPassword = model.OldPassword, NewPassword = model.NewPassword };
            var response = await _client.PostAsJsonAsync($"Accounts/change-password/{model.Id}", payload);

            if (response.IsSuccessStatusCode)
            {
                return Ok(new { icon = "success", message = "Password updated successfully!", redirectUrl = Url.Action("Index", "Home") });
            }

            var errorContent = await response.Content.ReadAsStringAsync();
            return BadRequest(new { message = $"Password update failed: {errorContent}" });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "SuperAdmin, Super Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            if (id <= 0)
                return BadRequest(new { message = "Invalid User ID." });

            var response = await _client.DeleteAsync($"Accounts/users/{id}");
            if (response.IsSuccessStatusCode)
            {
                return Ok(new { icon = "success", message = "User deleted successfully." });
            }

            return BadRequest(new { icon = "error", message = "Failed to delete user." });
        }

        private static ClaimsPrincipal CreateClaimsPrincipal(UsersVM userObj)
        {
            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, userObj.Id.ToString()),
                new(ClaimTypes.Name, !string.IsNullOrWhiteSpace(userObj.UserName) ? userObj.UserName : "User"),
                new(ClaimTypes.Email, userObj.Email ?? ""),
                new("Avatar", !string.IsNullOrWhiteSpace(userObj.Img) ? userObj.Img : "/images/Portrait_Placeholder.png")
            };

            var rolesList = new List<string>();

            if (userObj.Roles != null && userObj.Roles.Any())
            {
                rolesList.AddRange(userObj.Roles);
            }

            if (!string.IsNullOrWhiteSpace(userObj.RoleName))
            {
                rolesList.Add(userObj.RoleName);
            }

            if (!rolesList.Any())
            {
                rolesList.Add("User");
            }

            foreach (var role in rolesList.Distinct())
            {
                var cleanRole = role.Trim();
                var normalizedRole = cleanRole.Replace(" ", "");

                if (normalizedRole.Equals("superadmin", StringComparison.OrdinalIgnoreCase))
                {
                    claims.Add(new Claim(ClaimTypes.Role, "SuperAdmin"));
                    claims.Add(new Claim(ClaimTypes.Role, "Super Admin"));
                }
                else if (normalizedRole.Equals("admin", StringComparison.OrdinalIgnoreCase))
                {
                    claims.Add(new Claim(ClaimTypes.Role, "Admin"));
                }
                else
                {
                    claims.Add(new Claim(ClaimTypes.Role, cleanRole));
                }
            }

            return new ClaimsPrincipal(new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme));
        }

        private static MultipartFormDataContent BuildPersonalDataFormData(PersonalDataVM model)
        {
            var formData = new MultipartFormDataContent
            {
                { new StringContent(model.Id.ToString()), nameof(model.Id) },
                { new StringContent(model.UserName ?? string.Empty), nameof(model.UserName) },
                { new StringContent(model.Email ?? string.Empty), nameof(model.Email) },
                { new StringContent(model.NameEn ?? string.Empty), nameof(model.NameEn) },
                { new StringContent(model.NameAr ?? string.Empty), nameof(model.NameAr) },
                { new StringContent(model.Img ?? string.Empty), nameof(model.Img) }
            };

            if (!string.IsNullOrEmpty(model.RoleName))
                formData.Add(new StringContent(model.RoleName), nameof(model.RoleName));

            return formData;
        }

        private async Task<string> SaveUserImageAsync(IFormFile imgFile)
        {
            var fileName = $"{Guid.NewGuid()}{Path.GetExtension(imgFile.FileName)}";
            var uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "images", "users");

            if (!Directory.Exists(uploadsFolder))
                Directory.CreateDirectory(uploadsFolder);

            var fullPath = Path.Combine(uploadsFolder, fileName);
            await using var stream = new FileStream(fullPath, FileMode.Create);
            await imgFile.CopyToAsync(stream);

            return "/images/users/" + fileName;
        }
    }
}