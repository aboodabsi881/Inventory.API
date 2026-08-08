using Inventory.Core.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Inventory.Core.Interfaces
{
    public interface IAccountService
    {



        Task<UserResponseDto> GetUserByIdAsync(string id);
        Task<UserResponseDto> GetUserByEmailAsync(string email);
        Task<IReadOnlyList<UserResponseDto>> GetAllUsersAsync();
        Task<UserResponseDto?> SignInUserAsync(LoginRequestDto model);
        Task<UserResponseDto> CreateUserAsync(RegisterRequestDto model);
        Task<PersonalDataResponseDto> GetPersonalDataAsync(string id);
        Task<PersonalDataResponseDto> UpdateUserAsync(string id, PersonalDataRequestDto model);
        Task<bool> DeleteUserAsync(string id);
        Task<bool> AddUserToRoleAsync(string userId, string roleName);
        Task<bool> ChangePasswordAsync(string id, ChangePasswordRequestDto model);
        Task<bool> ResetPasswordAsync(string email, string token, string newPassword);
    }
}