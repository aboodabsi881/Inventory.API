using Inventory.Web.Resources;
using Inventory.Web.ViewModels.Roles;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using System.Text.Json;

namespace Inventory.Web.Controllers
{
    [Authorize(Roles = "SuperAdmin, Super Admin")]
    public class RolesController : Controller
    {
        private readonly HttpClient _client;
        private readonly IStringLocalizer<SharedResource> _localizer;
        private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

        public RolesController(
            IHttpClientFactory httpClientFactory,
            IStringLocalizer<SharedResource> localizer)
        {
            _client = httpClientFactory.CreateClient("InventoryAPI");
            _localizer = localizer;
        }

        public async Task<IActionResult> Index()
        {
            var roles = await _client.GetFromJsonAsync<List<RoleVM>>("Roles", JsonOptions) ?? new List<RoleVM>();
            return View(roles);
        }

        public IActionResult Create() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(RoleVM roleVM)
        {
            if (!ModelState.IsValid)
                return BadRequest(new { icon = "warning", message = _localizer["ValidationFailed"].Value });

            var response = await _client.PostAsJsonAsync("Roles", new { Name = roleVM.Name });
            if (response.IsSuccessStatusCode)
            {
                return Ok(new
                {
                    icon = "success",
                    message = _localizer["CreatedRole"].Value,
                    redirectUrl = Url.Action("Index")
                });
            }

            var errorDetails = await response.Content.ReadAsStringAsync();
            return BadRequest(new { icon = "error", message = $"API Error: {errorDetails}" });
        }

        public async Task<IActionResult> Edit(int id)
        {
            var role = await _client.GetFromJsonAsync<RoleVM>($"Roles/{id}", JsonOptions);
            if (role == null) return NotFound();

            return View(role);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, RoleVM roleVM)
        {
            if (id != roleVM.Id)
                return BadRequest(new { icon = "error", message = _localizer["NotFoundRoleId"].Value });

            if (!ModelState.IsValid)
                return BadRequest(new { icon = "warning", message = _localizer["ValidationFailed"].Value });

            var response = await _client.PutAsync($"Roles/{id}", JsonContent.Create(new { Id = roleVM.Id, Name = roleVM.Name }));
            if (response.IsSuccessStatusCode)
            {
                return Ok(new
                {
                    icon = "success",
                    message = _localizer["UpdatedRole"].Value,
                    redirectUrl = Url.Action("Index")
                });
            }

            var errorDetails = await response.Content.ReadAsStringAsync();
            return BadRequest(new { icon = "error", message = $"API Error: {errorDetails}" });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var role = await _client.GetFromJsonAsync<RoleVM>($"Roles/{id}", JsonOptions);
            if (role != null && (role.Name.Equals("SuperAdmin", StringComparison.OrdinalIgnoreCase) || role.Name.Equals("Super Admin", StringComparison.OrdinalIgnoreCase)))
            {
                return BadRequest(new { icon = "error", message = "System Role 'SuperAdmin' cannot be deleted!" });
            }

            var response = await _client.DeleteAsync($"Roles/{id}");
            if (response.IsSuccessStatusCode)
            {
                return Ok(new
                {
                    icon = "info",
                    message = _localizer["RoleDeleted"].Value,
                    deletedId = id,
                    redirectUrl = Url.Action("Index")
                });
            }

            return BadRequest(new { icon = "error", message = _localizer["NotFoundRole"].Value });
        }
    }
}