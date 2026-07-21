using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.Threading.Tasks;
using Inventory.Core.DTOs;

namespace Inventory.Core.Interfaces
{
    public interface IAccountService
    {
        // User Retrieval
        Task<UserResponseDto> GetUserByIdAsync(string id);
        Task<UserResponseDto> GetUserByEmailAsync(string email);
        Task<IReadOnlyList<UserResponseDto>> GetAllUsersAsync();

        // Authentication & Registration
        Task<UserResponseDto?> SignInUserAsync(LoginRequestDto model);
        Task<UserResponseDto> CreateUserAsync(RegisterRequestDto model);
        Task<bool> AddUserToRoleAsync(string userId, string roleName);

        // Profile Management
        Task<PersonalDataResponseDto> GetPersonalDataAsync(string id);
        Task<PersonalDataResponseDto> UpdateUserAsync(string id, PersonalDataRequestDto model, IFormFile? imgFile = null);
        Task<string> UpdateUserImgAsync(string id, IFormFile imgFile);
        Task<bool> ChangePasswordAsync(string id, ChangePasswordRequestDto model);

        // Account Administration
        Task<bool> ResetPasswordAsync(string email, string token, string newPassword);
        Task<bool> DeleteUserAsync(string id);
    }
}