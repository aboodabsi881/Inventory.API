using Microsoft.AspNetCore.Mvc;
using Inventory.Core.DTOs;
using Inventory.Core.Interfaces;

namespace Inventory.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AccountsController : ControllerBase
    {
        private readonly IAccountService _accountService;

        public AccountsController(IAccountService accountService)
        {
            _accountService = accountService;
        }

        // POST: api/accounts/register
        [HttpPost("register")]
        public async Task<ActionResult<UserResponseDto>> Register([FromBody] RegisterRequestDto model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var createdUser = await _accountService.CreateUserAsync(model);
                return CreatedAtAction(nameof(GetUserById), new { id = createdUser.Id }, createdUser);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // POST: api/accounts/login
        [HttpPost("login")]
        public async Task<ActionResult<UserResponseDto>> Login([FromBody] LoginRequestDto model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var user = await _accountService.SignInUserAsync(model);
            if (user == null)
                return Unauthorized(new { message = "Invalid username/email or password." });

            return Ok(user);
        }

        // GET: api/accounts/users
        [HttpGet("users")]
        public async Task<ActionResult<IReadOnlyList<UserResponseDto>>> GetAllUsers()
        {
            var users = await _accountService.GetAllUsersAsync();
            return Ok(users);
        }

        // GET: api/accounts/users/5
        [HttpGet("users/{id}")]
        public async Task<ActionResult<UserResponseDto>> GetUserById(string id)
        {
            try
            {
                var user = await _accountService.GetUserByIdAsync(id);
                return Ok(user);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

        // GET: api/accounts/personal-data/5
        [HttpGet("personal-data/{id}")]
        public async Task<ActionResult<PersonalDataResponseDto>> GetPersonalData(string id)
        {
            try
            {
                var data = await _accountService.GetPersonalDataAsync(id);
                return Ok(data);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

        // PUT: api/accounts/personal-data/5
        [HttpPut("personal-data/{id}")]
        public async Task<ActionResult<PersonalDataResponseDto>> UpdatePersonalData(
            string id,
            [FromForm] PersonalDataRequestDto model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var updatedUser = await _accountService.UpdateUserAsync(id, model);
                return Ok(updatedUser);
            }
            catch (Exception ex) when (ex is KeyNotFoundException || ex is InvalidOperationException)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // POST: api/accounts/change-password/5
        [HttpPost("change-password/{id}")]
        public async Task<IActionResult> ChangePassword(string id, [FromBody] ChangePasswordRequestDto model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                await _accountService.ChangePasswordAsync(id, model);
                return Ok(new { message = "Password updated successfully." });
            }
            catch (Exception ex) when (ex is KeyNotFoundException || ex is InvalidOperationException)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // DELETE: api/accounts/users/5
        [HttpDelete("users/{id}")]
        public async Task<IActionResult> DeleteUser(string id)
        {
            try
            {
                await _accountService.DeleteUserAsync(id);
                return NoContent();
            }
            catch (Exception ex) when (ex is KeyNotFoundException || ex is InvalidOperationException)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}