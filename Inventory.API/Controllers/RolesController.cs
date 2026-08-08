using Inventory.Core.DTOs;
using Inventory.Core.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Inventory.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RolesController : ControllerBase
    {
        private readonly IRoleService _roleService;

        public RolesController(IRoleService roleService)
        {
            _roleService = roleService;
        }

        // GET: api/roles
        [HttpGet]
        public async Task<ActionResult<IReadOnlyList<RoleResponseDto>>> GetAllRoles()
        {
            var roles = await _roleService.GetAllRolesAsync();
            return Ok(roles);
        }

        // GET: api/roles/5
        [HttpGet("{id:int}")]
        public async Task<ActionResult<RoleResponseDto>> GetRoleById(int id)
        {
            var role = await _roleService.GetRoleByIdAsync(id);
            if (role == null)
                return NotFound(new { message = $"Role with ID {id} was not found." });

            return Ok(role);
        }

        // POST: api/roles
        [HttpPost]
        public async Task<ActionResult<RoleResponseDto>> CreateRole([FromBody] RoleRequestDto model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var createdRole = await _roleService.CreateRoleAsync(model);
                return CreatedAtAction(nameof(GetRoleById), new { id = createdRole.Id }, createdRole);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // PUT: api/roles/5
        [HttpPut("{id:int}")]
        public async Task<IActionResult> UpdateRole(int id, [FromBody] RoleRequestDto model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                await _roleService.UpdateRoleAsync(id, model);
                return Ok(new { message = "Role updated successfully." });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // DELETE: api/roles/5
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DeleteRole(int id)
        {
            try
            {
                await _roleService.DeleteRoleAsync(id);
                return NoContent();
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}