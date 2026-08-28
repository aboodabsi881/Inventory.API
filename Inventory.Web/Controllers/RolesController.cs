using Inventory.Web.Interfaces;
using Inventory.Web.ViewModels.Roles;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using System.Threading.Tasks;

namespace Inventory.Web.Controllers
{
    [Authorize(Roles = "SuperAdmin, Super Admin")]
    public class RolesController : Controller
    {
        private readonly IViewRoleService _roleService;
        private readonly IStringLocalizer<SharedResource> _localizer;

        public RolesController(
            IViewRoleService roleService,
            IStringLocalizer<SharedResource> localizer)
        {
            _roleService = roleService;
            _localizer = localizer;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var roles = await _roleService.GetAllRolesAsync();
            return View(roles);
        }

        [HttpGet]
        public IActionResult Create() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(RoleVM roleVM)
        {
            if (!ModelState.IsValid)
                return BadRequest(new { icon = "warning", message = _localizer["ValidationFailed"].Value });

            var result = await _roleService.CreateRoleAsync(roleVM);
            if (result.Success)
            {
                return Ok(new
                {
                    icon = "success",
                    message = _localizer["CreatedRole"].Value,
                    redirectUrl = Url.Action("Index")
                });
            }

            return BadRequest(new { icon = "error", message = result.Message });
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            if (id <= 0) return BadRequest("Invalid Role ID.");

            var role = await _roleService.GetRoleByIdAsync(id);
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

            var result = await _roleService.UpdateRoleAsync(id, roleVM);
            if (result.Success)
            {
                return Ok(new
                {
                    icon = "success",
                    message = _localizer["UpdatedRole"].Value,
                    redirectUrl = Url.Action("Index")
                });
            }

            return BadRequest(new { icon = "error", message = result.Message });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (id <= 0) return BadRequest(new { icon = "error", message = _localizer["NotFoundRole"].Value });

            var result = await _roleService.DeleteRoleAsync(id);
            if (result.IsProtected)
            {
                return BadRequest(new { icon = "error", message = result.Message });
            }

            if (result.Success)
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