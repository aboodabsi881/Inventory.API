using Inventory.Web.Interfaces;
using Inventory.Web.ViewModels.Roles;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;

namespace Inventory.Web.Services
{
    public class ViewRoleService : IViewRoleService
    {
        private readonly HttpClient _client;
        private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

        public ViewRoleService(IHttpClientFactory httpClientFactory)
        {
            _client = httpClientFactory.CreateClient("InventoryAPI");
        }

        public async Task<List<RoleVM>> GetAllRolesAsync()
        {
            try
            {
                return await _client.GetFromJsonAsync<List<RoleVM>>("Roles", JsonOptions) ?? new List<RoleVM>();
            }
            catch
            {
                return new List<RoleVM>();
            }
        }

        public async Task<RoleVM?> GetRoleByIdAsync(int id)
        {
            try
            {
                return await _client.GetFromJsonAsync<RoleVM>($"Roles/{id}", JsonOptions);
            }
            catch
            {
                return null;
            }
        }

        public async Task<(bool Success, string Message)> CreateRoleAsync(RoleVM roleVM)
        {
            var response = await _client.PostAsJsonAsync("Roles", new { Name = roleVM.Name });
            if (response.IsSuccessStatusCode)
            {
                return (true, string.Empty);
            }

            var errorDetails = await response.Content.ReadAsStringAsync();
            return (false, $"API Error: {errorDetails}");
        }

        public async Task<(bool Success, string Message)> UpdateRoleAsync(int id, RoleVM roleVM)
        {
            var response = await _client.PutAsync($"Roles/{id}", JsonContent.Create(new { Id = roleVM.Id, Name = roleVM.Name }));
            if (response.IsSuccessStatusCode)
            {
                return (true, string.Empty);
            }

            var errorDetails = await response.Content.ReadAsStringAsync();
            return (false, $"API Error: {errorDetails}");
        }

        public async Task<(bool Success, bool IsProtected, string Message)> DeleteRoleAsync(int id)
        {
            var role = await GetRoleByIdAsync(id);
            if (role != null && (role.Name.Equals("SuperAdmin", StringComparison.OrdinalIgnoreCase) ||
                                role.Name.Equals("Super Admin", StringComparison.OrdinalIgnoreCase)))
            {
                return (false, true, "System Role 'SuperAdmin' cannot be deleted!");
            }

            var response = await _client.DeleteAsync($"Roles/{id}");
            if (response.IsSuccessStatusCode)
            {
                return (true, false, string.Empty);
            }

            var errorDetails = await response.Content.ReadAsStringAsync();
            return (false, false, errorDetails);
        }
    }
}