using Inventory.ViewModel.Roles;
using Inventory.Web.ViewModels.Accounts;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;

namespace Inventory.Web.Controllers
{
    public class AccountsController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public AccountsController(IHttpClientFactory httpClientFactory, IWebHostEnvironment webHostEnvironment)
        {
            _httpClientFactory = httpClientFactory;
            _webHostEnvironment = webHostEnvironment;
        }

        // ==========================================
        // 1️⃣ GET: Accounts/Users (Users List Index)
        // ==========================================
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var client = _httpClientFactory.CreateClient("InventoryAPI");
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

            var users = await client.GetFromJsonAsync<List<UsersVM>>("Accounts/users", options) ?? new List<UsersVM>();

            return View(users);
        }

        // ==========================================
        // 2️⃣ LOGIN
        // ==========================================
        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginVM model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var client = _httpClientFactory.CreateClient("InventoryAPI");
            var response = await client.PostAsJsonAsync("Accounts/login", model);

            if (response.IsSuccessStatusCode)
            {
                var userObj = await response.Content.ReadFromJsonAsync<UsersVM>(new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                if (userObj != null)
                {
                    // 💡 Save user profile avatar & name into Cookies for global access in Layout
                    SetUserCookies(userObj.Img, userObj.UserName);
                }

                return Json(new { icon = "success", message = "Login successful!", redirectUrl = Url.Action("Index", "Home") });
            }

            return BadRequest(new { message = "Invalid username/email or password." });
        }

        // ==========================================
        // 3️⃣ LOGOUT
        // ==========================================
        [HttpGet]
        public IActionResult Logout()
        {
            // Clear layout user cookies
            Response.Cookies.Delete("UserAvatar");
            Response.Cookies.Delete("UserName");

            return RedirectToAction(nameof(Login));
        }

        // ==========================================
        // 4️⃣ REGISTER
        // ==========================================
        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterVM model)
        {
            if (!ModelState.IsValid)
                return View(model);

            string? imagePath = null;

            // Save uploaded image to wwwroot/images/users if provided
            if (model.ImgFile != null && model.ImgFile.Length > 0)
            {
                imagePath = await SaveUserImageAsync(model.ImgFile);
            }

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

            var client = _httpClientFactory.CreateClient("InventoryAPI");
            var response = await client.PostAsJsonAsync("Accounts/register", registerPayload);

            if (response.IsSuccessStatusCode)
            {
                return Json(new { icon = "success", message = "User registered successfully!", redirectUrl = Url.Action("Index") });
            }

            var errorContent = await response.Content.ReadAsStringAsync();
            return BadRequest(new { message = $"Registration failed: {errorContent}" });
        }

        // ==========================================
        // 5️⃣ PERSONAL DATA (USER PROFILE)
        // ==========================================
        [HttpGet]
        public async Task<IActionResult> PersonalData(int id)
        {
            if (id <= 0)
                return BadRequest("Invalid User ID.");

            var client = _httpClientFactory.CreateClient("InventoryAPI");
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

            var response = await client.GetAsync($"Accounts/personal-data/{id}");
            if (!response.IsSuccessStatusCode)
                return NotFound();

            var vm = await response.Content.ReadFromJsonAsync<PersonalDataVM>(options);
            if (vm == null)
                return NotFound();

            var rolesList = await client.GetFromJsonAsync<List<RoleVM>>("Roles", options) ?? new List<RoleVM>();
            vm.RoleLookup = new SelectList(rolesList.Select(r => r.Name), vm.RoleName);

            // Refresh cookies with user data
            SetUserCookies(vm.Img, vm.UserName);

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PersonalData(PersonalDataVM model)
        {
            var client = _httpClientFactory.CreateClient("InventoryAPI");
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

            if (!ModelState.IsValid)
            {
                var rolesList = await client.GetFromJsonAsync<List<RoleVM>>("Roles", options) ?? new List<RoleVM>();
                model.RoleLookup = new SelectList(rolesList.Select(r => r.Name), model.RoleName);
                return View(model);
            }

            // 1️⃣ Save new uploaded image if provided
            if (model.ImgFile != null && model.ImgFile.Length > 0)
            {
                model.Img = await SaveUserImageAsync(model.ImgFile);
            }
            // 2️⃣ Safeguard: Fallback to placeholder if image path remains empty
            else if (string.IsNullOrWhiteSpace(model.Img))
            {
                model.Img = "/images/Portrait_Placeholder.png";
            }

            // 3️⃣ Pack payload as MultipartFormDataContent because API expects [FromForm]
            using var formData = new MultipartFormDataContent();
            formData.Add(new StringContent(model.Id.ToString()), nameof(model.Id));
            formData.Add(new StringContent(model.UserName ?? string.Empty), nameof(model.UserName));
            formData.Add(new StringContent(model.Email ?? string.Empty), nameof(model.Email));
            formData.Add(new StringContent(model.NameEn ?? string.Empty), nameof(model.NameEn));
            formData.Add(new StringContent(model.NameAr ?? string.Empty), nameof(model.NameAr));

            if (!string.IsNullOrEmpty(model.RoleName))
                formData.Add(new StringContent(model.RoleName), nameof(model.RoleName));

            formData.Add(new StringContent(model.Img), nameof(model.Img));

            var response = await client.PutAsync($"Accounts/personal-data/{model.Id}", formData);

            if (response.IsSuccessStatusCode)
            {
                // 💡 Immediately update cookies so new avatar displays across all pages
                SetUserCookies(model.Img, model.UserName);

                return Json(new { icon = "success", message = "Profile updated successfully!", redirectUrl = Url.Action("Index") });
            }

            var errorContent = await response.Content.ReadAsStringAsync();
            return BadRequest(new { icon = "error", message = !string.IsNullOrEmpty(errorContent) ? errorContent : "Failed to update profile." });
        }

        // ==========================================
        // 6️⃣ CHANGE PASSWORD
        // ==========================================
        [HttpGet]
        public IActionResult ChangePassword(int id)
        {
            if (id <= 0)
                return BadRequest("User ID is required.");

            var model = new ChangePasswordVM { Id = id.ToString() };
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(ChangePasswordVM model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var client = _httpClientFactory.CreateClient("InventoryAPI");

            var payload = new
            {
                CurrentPassword = model.OldPassword,
                NewPassword = model.NewPassword
            };

            var response = await client.PostAsJsonAsync($"Accounts/change-password/{model.Id}", payload);

            if (response.IsSuccessStatusCode)
            {
                return Json(new { icon = "success", message = "Password updated successfully!", redirectUrl = Url.Action("Index") });
            }

            var errorContent = await response.Content.ReadAsStringAsync();
            return BadRequest(new { message = $"Password update failed: {errorContent}" });
        }

        // ==========================================
        // 7️⃣ DELETE USER
        // ==========================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            if (id <= 0)
                return BadRequest(new { message = "Invalid User ID." });

            var client = _httpClientFactory.CreateClient("InventoryAPI");
            var response = await client.DeleteAsync($"Accounts/users/{id}");

            if (response.IsSuccessStatusCode)
            {
                return Json(new { icon = "success", message = "User deleted successfully." });
            }

            return BadRequest(new { icon = "error", message = "Failed to delete user." });
        }

        // ==========================================
        // 💡 HELPER METHODS
        // ==========================================
        private async Task<string> SaveUserImageAsync(IFormFile imgFile)
        {
            var fileName = $"{Guid.NewGuid()}{Path.GetExtension(imgFile.FileName)}";
            var uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "images", "users");

            if (!Directory.Exists(uploadsFolder))
            {
                Directory.CreateDirectory(uploadsFolder);
            }

            var fullPath = Path.Combine(uploadsFolder, fileName);
            using (var stream = new FileStream(fullPath, FileMode.Create))
            {
                await imgFile.CopyToAsync(stream);
            }

            return "/images/users/" + fileName;
        }

        private void SetUserCookies(string? imgPath, string? userName)
        {
            var cookieOptions = new CookieOptions
            {
                Expires = DateTime.Now.AddDays(7),
                HttpOnly = false, // Allows layout to read safely
                IsEssential = true
            };

            var avatarUrl = !string.IsNullOrWhiteSpace(imgPath) ? imgPath : "/images/Portrait_Placeholder.png";
            var name = !string.IsNullOrWhiteSpace(userName) ? userName : "User";

            Response.Cookies.Append("UserAvatar", avatarUrl, cookieOptions);
            Response.Cookies.Append("UserName", name, cookieOptions);
        }
    }


}