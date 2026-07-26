using Inventory.ViewModel.Roles;
using Inventory.Web.Resources;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using System.Net.Http.Json;

namespace Inventory.Web.Controllers
{
    public class RolesController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IStringLocalizer<SharedResource> _localizer;

        public RolesController(
            IHttpClientFactory httpClientFactory,
            IStringLocalizer<SharedResource> localizer)
        {
            _httpClientFactory = httpClientFactory;
            _localizer = localizer;
        }

        // GET: ApplicationRoles
        // Retrieves and displays all roles from the API.
        public async Task<IActionResult> Index()
        {
            var client = _httpClientFactory.CreateClient("InventoryAPI");
            var roles = await client.GetFromJsonAsync<List<RoleVM>>("Roles");

            return View(roles ?? new List<RoleVM>());
        }

        // GET: ApplicationRoles/Create
        // Renders the form to create a new role.
        public IActionResult Create()
        {
            return View();
        }

        // POST: ApplicationRoles/Create
        // Posts role data to the API.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(RoleVM roleVM)
        {
            if (!ModelState.IsValid)
                return BadRequest(new { icon = "warning", message = _localizer["ValidationFailed"].Value });

            var client = _httpClientFactory.CreateClient("InventoryAPI");
            var response = await client.PostAsJsonAsync("Roles", new { Name = roleVM.Name });

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

        // GET: ApplicationRoles/Edit/5
        // Retrieves existing role details to populate the edit form.
        public async Task<IActionResult> Edit(int id)
        {
            var client = _httpClientFactory.CreateClient("InventoryAPI");
            var role = await client.GetFromJsonAsync<RoleVM>($"Roles/{id}");

            if (role == null)
                return NotFound();

            return View(role);
        }

        // POST: ApplicationRoles/Edit/5
        // Updates role data via the API.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, RoleVM roleVM)
        {
            if (id != roleVM.Id)
                return BadRequest(new { icon = "error", message = _localizer["NotFoundRoleId"].Value });

            if (!ModelState.IsValid)
                return BadRequest(new { icon = "warning", message = _localizer["ValidationFailed"].Value });

            var client = _httpClientFactory.CreateClient("InventoryAPI");
            var response = await client.PutAsJsonAsync($"Roles/{id}", new { Id = roleVM.Id, Name = roleVM.Name });

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

        // POST: ApplicationRoles/Delete/5
        // Deletes role from API.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var client = _httpClientFactory.CreateClient("InventoryAPI");
            var response = await client.DeleteAsync($"Roles/{id}");

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