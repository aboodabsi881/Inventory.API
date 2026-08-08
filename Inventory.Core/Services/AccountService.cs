using Inventory.Core.DTOs;
using Inventory.Core.Entities.Users.ApplicationRoles;
using Inventory.Core.Entities.Users.ApplicationUsers;
using Inventory.Core.Interfaces;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Inventory.Core.Services
{
    public class AccountService : IAccountService
    {
        private readonly IRepository<ApplicationUser> _appUserRepo;
        private readonly IRepository<ApplicationRole> _roleRepo;
        private readonly IRepository<IdentityUserRole<int>> _userRoleRepo;

        public AccountService(
            IRepository<ApplicationUser> appUserRepo,
            IRepository<ApplicationRole> roleRepo,
            IRepository<IdentityUserRole<int>> userRoleRepo)
        {
            _appUserRepo = appUserRepo;
            _roleRepo = roleRepo;
            _userRoleRepo = userRoleRepo;
        }

        public async Task<UserResponseDto> GetUserByIdAsync(string id)
        {
            if (!int.TryParse(id, out int userId))
                throw new ArgumentException("Invalid User ID.");

            var dto = await _appUserRepo.GetDtoByIdAsync<UserResponseDto>(userId);
            if (dto == null)
                throw new KeyNotFoundException($"User with ID {id} was not found.");

            var roles = await GetRolesForUserAsync(userId);
            dto.Roles = roles;
            dto.RoleName = roles.FirstOrDefault() ?? "User";
            return dto;
        }

        public async Task<UserResponseDto> GetUserByEmailAsync(string email)
        {
            var user = await _appUserRepo.GetFirstOrDefaultAsync(u => u.Email == email);
            if (user == null)
                throw new KeyNotFoundException($"User with email {email} was not found.");

            var dto = await _appUserRepo.GetDtoByIdAsync<UserResponseDto>(user.Id);
            if (dto != null)
            {
                var roles = await GetRolesForUserAsync(user.Id);
                dto.Roles = roles;
                dto.RoleName = roles.FirstOrDefault() ?? "User";
            }
            return dto!;
        }

        public async Task<IReadOnlyList<UserResponseDto>> GetAllUsersAsync()
        {
            var dtos = await _appUserRepo.GetAllDtoAsync<UserResponseDto>();
            var userRoles = await _userRoleRepo.GetAllAsync();
            var roles = await _roleRepo.GetAllAsync();

            var roleDict = roles.ToDictionary(r => r.Id, r => r.Name ?? string.Empty);
            var userRoleLookup = userRoles
                .GroupBy(ur => ur.UserId)
                .ToDictionary(
                    g => g.Key,
                    g => g.Select(ur => roleDict.TryGetValue(ur.RoleId, out var name) ? name : null)
                          .Where(n => !string.IsNullOrEmpty(n))
                          .Cast<string>()
                          .ToList()
                );

            foreach (var dto in dtos)
            {
                if (userRoleLookup.TryGetValue(dto.Id, out var rolesList))
                {
                    dto.Roles = rolesList;
                    dto.RoleName = rolesList.FirstOrDefault() ?? "User";
                }
            }

            return dtos;
        }

        public async Task<UserResponseDto?> SignInUserAsync(LoginRequestDto model)
        {
            var user = await _appUserRepo.GetFirstOrDefaultAsync(u => u.Email == model.UserName || u.UserName == model.UserName);
            if (user == null) return null;

            var dto = await _appUserRepo.GetDtoByIdAsync<UserResponseDto>(user.Id);
            if (dto != null)
            {
                var roles = await GetRolesForUserAsync(user.Id);
                dto.Roles = roles;
                dto.RoleName = roles.FirstOrDefault() ?? "User"; // Fixed: Set RoleName on login
            }
            return dto;
        }

        public async Task<UserResponseDto> CreateUserAsync(RegisterRequestDto model)
        {
            var dto = await _appUserRepo.CreateFromDtoAsync<RegisterRequestDto, UserResponseDto>(model);

            var roleName = string.IsNullOrEmpty(model.RoleName) ? "User" : model.RoleName;
            await AddUserToRoleByNameAsync(dto.Id, roleName);

            dto.Roles = new List<string> { roleName };
            dto.RoleName = roleName;
            return dto;
        }

        public async Task<PersonalDataResponseDto> GetPersonalDataAsync(string id)
        {
            if (!int.TryParse(id, out int userId))
                throw new ArgumentException("Invalid User ID.");

            var dto = await _appUserRepo.GetDtoByIdAsync<PersonalDataResponseDto>(userId);
            if (dto == null)
                throw new KeyNotFoundException($"User with ID {id} was not found.");

            var roles = await GetRolesForUserAsync(userId);
            dto.RoleName = roles.FirstOrDefault() ?? "User";

            return dto;
        }

        public async Task<PersonalDataResponseDto> UpdateUserAsync(string id, PersonalDataRequestDto model)
        {
            if (!int.TryParse(id, out int userId))
                throw new ArgumentException("Invalid User ID.");

            var dto = await _appUserRepo.UpdateFromDtoAsync<PersonalDataRequestDto, PersonalDataResponseDto>(userId, model);

            if (!string.IsNullOrEmpty(model.RoleName))
            {
                var existingUserRoles = (await _userRoleRepo.GetAllAsync()).Where(ur => ur.UserId == userId).ToList();
                foreach (var ur in existingUserRoles)
                {
                    _userRoleRepo.Delete(ur);
                }
                await _userRoleRepo.SaveChangesAsync();

                await AddUserToRoleByNameAsync(userId, model.RoleName);
                dto.RoleName = model.RoleName;
            }

            return dto;
        }

        public async Task<bool> DeleteUserAsync(string id)
        {
            if (!int.TryParse(id, out int userId))
                throw new ArgumentException("Invalid User ID.");

            return await _appUserRepo.DeleteAndSaveAsync(userId);
        }

        public async Task<bool> AddUserToRoleAsync(string userId, string roleName)
        {
            if (!int.TryParse(userId, out int uId)) return false;
            return await AddUserToRoleByNameAsync(uId, roleName);
        }

        public async Task<bool> ChangePasswordAsync(string id, ChangePasswordRequestDto model) => await Task.FromResult(true);
        public async Task<bool> ResetPasswordAsync(string email, string token, string newPassword) => await Task.FromResult(true);

        private async Task<List<string>> GetRolesForUserAsync(int userId)
        {
            var userRoles = (await _userRoleRepo.GetAllAsync()).Where(ur => ur.UserId == userId).Select(ur => ur.RoleId).ToList();
            var roles = (await _roleRepo.GetAllAsync()).Where(r => userRoles.Contains(r.Id)).Select(r => r.Name ?? string.Empty).ToList();
            return roles;
        }

        private async Task<bool> AddUserToRoleByNameAsync(int userId, string roleName)
        {
            var role = (await _roleRepo.GetAllAsync()).FirstOrDefault(r => r.Name == roleName);
            if (role == null) return false;

            await _userRoleRepo.AddAsync(new IdentityUserRole<int>
            {
                UserId = userId,
                RoleId = role.Id
            });
            return await _userRoleRepo.SaveChangesAsync() > 0;
        }
    }
}