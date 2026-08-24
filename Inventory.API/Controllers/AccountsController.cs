using Microsoft.AspNetCore.Mvc;
using Inventory.Core.DTOs;
using Inventory.Core.Interfaces;

namespace Inventory.API.Controllers
{
    [ApiController] // Indicates that this controller responds to web API requests and enables automatic model validation and other API-specific behaviors.
    [Route("api/[controller]")] // Defines the route template for the controller. so this route will be "api/accounts".
    // [] its call atribute
    public class AccountsController : ControllerBase // ControllerBase is for API controllers that do not need view support, while Controller is for MVC controllers that return views.
    {
        private readonly IAccountService _accountService;

        public AccountsController(IAccountService accountService)
        {
            _accountService = accountService;
        }

        [HttpPost("register")]
        public async Task<ActionResult<UserResponseDto>> Register([FromBody] RegisterRequestDto model) //FormBody attribute indicates that the model should be bound from the request body, typically in JSON format.
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

        [HttpGet("users")]
        public async Task<ActionResult<IReadOnlyList<UserResponseDto>>> GetAllUsers()
        {
            var users = await _accountService.GetAllUsersAsync();
            return Ok(users);
        }

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

        [HttpPatch("change-password/{id:int}")]
        public async Task<IActionResult> ChangePassword(int id, [FromBody] ChangePasswordRequestDto model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var result = await _accountService.ChangePasswordAsync(id.ToString(), model);
                if (result)
                    return Ok(new { message = "Password updated successfully." });

                return BadRequest(new { message = "Failed to update password." });
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