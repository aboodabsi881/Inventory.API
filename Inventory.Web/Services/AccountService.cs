using Inventory.Web.Interfaces;
using Inventory.Web.ViewModels.Accounts;
using Inventory.Web.ViewModels.Roles;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.PowerBI.Api.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text.Json;
using System.Threading.Tasks;

namespace Inventory.Web.Services
{
    public class ViewAccountService : IViewAccountService
    {
        private readonly HttpClient _client;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

        public ViewAccountService(IHttpClientFactory httpClientFactory, IWebHostEnvironment webHostEnvironment)
        {
            _client = httpClientFactory.CreateClient("InventoryAPI");
            _webHostEnvironment = webHostEnvironment;
        }

        public async Task<List<UsersVM>> GetAllUsersAsync()
        {
            return await _client.GetFromJsonAsync<List<UsersVM>>("Accounts/users", JsonOptions)
                   ?? new List<UsersVM>();
        }

        public async Task<(bool Success, UsersVM? User, string Message)> LoginAsync(LoginVM model)
        {
            var response = await _client.PostAsJsonAsync("Accounts/login", model);
            if (!response.IsSuccessStatusCode)
            {
                return (false, null, "Invalid username or password.");
            }

            var userObj = await response.Content.ReadFromJsonAsync<UsersVM>(JsonOptions);
            return userObj == null
                ? (false, null, "Invalid user response from server.")
                : (true, userObj, "Login successful!");
        }

        public async Task<(bool Success, string Message)> RegisterAsync(RegisterVM model)
        {
            string? imagePath = model.ImgFile is { Length: > 0 }
                ? await SaveUserImageAsync(model.ImgFile)
                : null;

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
                return (true, "User registered successfully!");
            }

            var errorContent = await response.Content.ReadAsStringAsync();
            return (false, ParseErrorMessage(errorContent, "Registration failed."));
        }

        public async Task<PersonalDataVM?> GetPersonalDataAsync(int id)
        {
            var response = await _client.GetAsync($"Accounts/personal-data/{id}");
            if (!response.IsSuccessStatusCode) return null;

            var vm = await response.Content.ReadFromJsonAsync<PersonalDataVM>(JsonOptions);
            if (vm == null) return null;

            var rolesList = await _client.GetFromJsonAsync<List<RoleVM>>("Roles", JsonOptions)
                            ?? new List<RoleVM>();
            vm.RoleLookup = new SelectList(rolesList.Select(r => r.Name), vm.RoleName);

            return vm;
        }

        public async Task<(bool Success, string Message)> UpdatePersonalDataAsync(PersonalDataVM model)
        {
            if (model.ImgFile is { Length: > 0 })
            {
                model.Img = await SaveUserImageAsync(model.ImgFile);
            }
            else if (string.IsNullOrWhiteSpace(model.Img))
            {
                model.Img = "/images/Portrait_Placeholder.png";
            }

            using var formData = new MultipartFormDataContent
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

            var response = await _client.PutAsync($"Accounts/personal-data/{model.Id}", formData);
            if (response.IsSuccessStatusCode)
            {
                return (true, "Profile updated successfully!");
            }

            var errorContent = await response.Content.ReadAsStringAsync();
            return (false, ParseErrorMessage(errorContent, "Failed to update profile."));
        }

        public async Task<(bool Success, string Message)> ChangePasswordAsync(ChangePasswordVM model)
        {
            var payload = new
            {
                CurrentPassword = model.CurrentPassword,
                NewPassword = model.NewPassword,
                ConfirmPassword = model.ConfirmPassword
            };

            var response = await _client.PutAsJsonAsync($"Accounts/change-password/{model.Id}", payload);
            if (response.IsSuccessStatusCode)
            {
                return (true, "Password updated successfully!");
            }

            var errorContent = await response.Content.ReadAsStringAsync();
            return (false, ParseErrorMessage(errorContent, "Failed to update password."));
        }

        public async Task<(bool Success, string Message)> DeleteUserAsync(int id)
        {
            var response = await _client.DeleteAsync($"Accounts/users/{id}");
            if (response.IsSuccessStatusCode)
            {
                return (true, "User deleted successfully.");
            }

            var errorContent = await response.Content.ReadAsStringAsync();
            return (false, ParseErrorMessage(errorContent, "Failed to delete user."));
        }

        public async Task<string> SaveUserImageAsync(IFormFile imgFile)
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

        public ClaimsPrincipal CreateClaimsPrincipal(UsersVM userObj)
        {
            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, userObj.Id.ToString()),
                new(ClaimTypes.Name, !string.IsNullOrWhiteSpace(userObj.UserName) ? userObj.UserName : "User"),
                new(ClaimTypes.Email, userObj.Email ?? string.Empty),
                new("NameAr", userObj.NameAr ?? string.Empty),
                new("NameEn", userObj.NameEn ?? string.Empty),
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

        private static string ParseErrorMessage(string errorContent, string fallback)
        {
            if (string.IsNullOrWhiteSpace(errorContent)) return fallback;

            try
            {
                using var doc = JsonDocument.Parse(errorContent);
                var root = doc.RootElement;

                if (root.TryGetProperty("errors", out var errorsProp) && errorsProp.ValueKind == JsonValueKind.Object)
                {
                    var errList = new List<string>();
                    foreach (var prop in errorsProp.EnumerateObject())
                    {
                        if (prop.Value.ValueKind == JsonValueKind.Array)
                        {
                            foreach (var err in prop.Value.EnumerateArray())
                            {
                                errList.Add(err.GetString() ?? "");
                            }
                        }
                        else if (prop.Value.ValueKind == JsonValueKind.String)
                        {
                            errList.Add(prop.Value.GetString() ?? "");
                        }
                    }
                    if (errList.Any()) return string.Join("<br/>", errList);
                }
                else if (root.TryGetProperty("message", out var msgProp))
                {
                    return msgProp.GetString() ?? fallback;
                }
            }
            catch
            {
                return errorContent;
            }

            return fallback;
        }
    }
}