using Inventory.Core.DTOs;
using Inventory.Core.Entities.Users.ApplicationUsers;
using Inventory.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Inventory.Core.Services
{
    public class AccountService : IAccountService
    {
        private readonly IRepository<ApplicationUser> _userRepo;

        public AccountService(IRepository<ApplicationUser> userRepo)
        {
            _userRepo = userRepo;
        }

        public async Task<UserResponseDto?> SignInUserAsync(LoginRequestDto model)
        {
            return await _userRepo.SignInUserAsync(model);
        }

        public async Task<UserResponseDto> CreateUserAsync(RegisterRequestDto model)
        {
            return await _userRepo.CreateUserFromDtoAsync(model);
        }

        public async Task<PersonalDataResponseDto> GetPersonalDataAsync(string id)
        {
            if (!int.TryParse(id, out int userId))
                throw new ArgumentException("Invalid User ID.");

            return await _userRepo.GetPersonalDataDtoAsync(userId);
        }

        public async Task<UserResponseDto> GetUserByIdAsync(string id)
        {
            if (!int.TryParse(id, out int userId))
                throw new ArgumentException("Invalid User ID.");

            return await _userRepo.GetUserWithRolesByIdAsync(userId);
        }

        public async Task<UserResponseDto> GetUserByEmailAsync(string email)
        {
            return await _userRepo.GetUserWithRolesByEmailAsync(email);
        }

        public async Task<IReadOnlyList<UserResponseDto>> GetAllUsersAsync()
        {
            return await _userRepo.GetAllUsersWithRolesAsync();
        }

        public async Task<PersonalDataResponseDto> UpdateUserAsync(string id, PersonalDataRequestDto model)
        {
            if (!int.TryParse(id, out int userId))
                throw new ArgumentException("Invalid User ID.");

            return await _userRepo.UpdatePersonalDataFromDtoAsync(userId, model);
        }

        public async Task<bool> DeleteUserAsync(string id)
        {
            if (!int.TryParse(id, out int userId))
                throw new ArgumentException("Invalid User ID.");

            return await _userRepo.DeleteAndSaveAsync(userId);
        }

        public async Task<bool> AddUserToRoleAsync(string userId, string roleName)
        {
            if (!int.TryParse(userId, out int uId)) return false;
            return await _userRepo.AddUserToRoleAsync(uId, roleName);
        }

        public async Task<bool> ChangePasswordAsync(string id, ChangePasswordRequestDto model)
        {
            if (!int.TryParse(id, out int userId))
                throw new ArgumentException("Invalid User ID.");

            return await _userRepo.ChangePasswordAsync(userId, model);
        }

        public async Task<bool> ResetPasswordAsync(string email, string token, string newPassword)
        {
            return await _userRepo.ResetPasswordAsync(email, token, newPassword);
        }
    }
}