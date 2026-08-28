using AutoMapper;
using Inventory.Core.DTOs;
using Inventory.Core.Entities.Users.ApplicationRoles;
using Inventory.Core.Entities.Users.ApplicationUsers;
using Inventory.Core.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Inventory.Core.Services
{
    public class AccountService : IAccountService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<ApplicationRole> _roleManager;
        private readonly IMapper _mapper;

        public AccountService(
            UserManager<ApplicationUser> userManager,
            RoleManager<ApplicationRole> roleManager,
            IMapper mapper)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _mapper = mapper;
        }

        public async Task<UserResponseDto?> SignInUserAsync(LoginRequestDto model)
        {
            var user = await _userManager.FindByEmailAsync(model.Username)
                       ?? await _userManager.FindByNameAsync(model.Username);

            if (user == null) return null;

            // Validate password securely
            var isPasswordValid = await _userManager.CheckPasswordAsync(user, model.Password);
            if (!isPasswordValid) return null;

            var dto = _mapper.Map<UserResponseDto>(user);
            var roles = await _userManager.GetRolesAsync(user);

            dto.Roles = roles.ToList();
            dto.RoleName = roles.FirstOrDefault() ?? "User";

            return dto;
        }

        public async Task<UserResponseDto> CreateUserAsync(RegisterRequestDto model)
        {
            var existingByEmail = await _userManager.FindByEmailAsync(model.Email);
            if (existingByEmail != null)
                throw new InvalidOperationException($"The email '{model.Email}' is already registered.");

            var existingByName = await _userManager.FindByNameAsync(model.Username);
            if (existingByName != null)
                throw new InvalidOperationException($"The username '{model.Username}' is already taken.");

            var user = _mapper.Map<ApplicationUser>(model);

            // Creates user and hashes the password automatically
            var result = await _userManager.CreateAsync(user, model.Password);
            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                throw new InvalidOperationException($"Registration failed: {errors}");
            }

            var roleName = string.IsNullOrWhiteSpace(model.RoleName) ? "User" : model.RoleName;

            // Ensure role exists
            if (!await _roleManager.RoleExistsAsync(roleName))
            {
                await _roleManager.CreateAsync(new ApplicationRole { Name = roleName });
            }

            await _userManager.AddToRoleAsync(user, roleName);

            var dto = _mapper.Map<UserResponseDto>(user);
            dto.RoleName = roleName;
            dto.Roles = new List<string> { roleName };

            return dto;
        }

        public async Task<PersonalDataResponseDto> GetPersonalDataAsync(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
                throw new KeyNotFoundException($"User with ID {id} was not found.");

            var dto = _mapper.Map<PersonalDataResponseDto>(user);
            var roles = await _userManager.GetRolesAsync(user);
            dto.RoleName = roles.FirstOrDefault() ?? "User";

            return dto;
        }

        public async Task<UserResponseDto> GetUserByIdAsync(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
                throw new KeyNotFoundException($"User with ID {id} was not found.");

            var dto = _mapper.Map<UserResponseDto>(user);
            var roles = await _userManager.GetRolesAsync(user);
            dto.Roles = roles.ToList();
            dto.RoleName = roles.FirstOrDefault() ?? "User";

            return dto;
        }

        public async Task<UserResponseDto> GetUserByEmailAsync(string email)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
                throw new KeyNotFoundException($"User with email '{email}' was not found.");

            var dto = _mapper.Map<UserResponseDto>(user);
            var roles = await _userManager.GetRolesAsync(user);
            dto.Roles = roles.ToList();
            dto.RoleName = roles.FirstOrDefault() ?? "User";

            return dto;
        }

        public async Task<IReadOnlyList<UserResponseDto>> GetAllUsersAsync()
        {
            var users = await _userManager.Users.AsNoTracking().ToListAsync();
            var dtos = new List<UserResponseDto>();

            foreach (var user in users)
            {
                var dto = _mapper.Map<UserResponseDto>(user);
                var roles = await _userManager.GetRolesAsync(user);
                dto.Roles = roles.ToList();
                dto.RoleName = roles.FirstOrDefault() ?? "User";
                dtos.Add(dto);
            }

            return dtos;
        }

        public async Task<PersonalDataResponseDto> UpdateUserAsync(string id, PersonalDataRequestDto model)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
                throw new KeyNotFoundException($"User with ID {id} was not found.");

            _mapper.Map(model, user);

            if (!string.IsNullOrWhiteSpace(model.Username))
            {
                user.UserName = model.Username;
            }

            if (!string.IsNullOrWhiteSpace(model.Email))
            {
                user.Email = model.Email;
            }

            var updateResult = await _userManager.UpdateAsync(user);
            if (!updateResult.Succeeded)
            {
                var errors = string.Join(", ", updateResult.Errors.Select(e => e.Description));
                throw new InvalidOperationException($"Update failed: {errors}");
            }

            if (!string.IsNullOrWhiteSpace(model.RoleName))
            {
                var currentRoles = await _userManager.GetRolesAsync(user);
                await _userManager.RemoveFromRolesAsync(user, currentRoles);

                if (!await _roleManager.RoleExistsAsync(model.RoleName))
                {
                    await _roleManager.CreateAsync(new ApplicationRole { Name = model.RoleName });
                }

                await _userManager.AddToRoleAsync(user, model.RoleName);
            }

            var dto = _mapper.Map<PersonalDataResponseDto>(user);
            var roles = await _userManager.GetRolesAsync(user);
            dto.RoleName = roles.FirstOrDefault() ?? (model.RoleName ?? "User");

            return dto;
        }

        public async Task<bool> DeleteUserAsync(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
                throw new KeyNotFoundException($"User with ID {id} was not found.");

            var result = await _userManager.DeleteAsync(user);
            return result.Succeeded;
        }

        public async Task<bool> AddUserToRoleAsync(string userId, string roleName)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return false;

            if (!await _roleManager.RoleExistsAsync(roleName))
            {
                await _roleManager.CreateAsync(new ApplicationRole { Name = roleName });
            }

            var result = await _userManager.AddToRoleAsync(user, roleName);
            return result.Succeeded;
        }

        public async Task<bool> ChangePasswordAsync(string id, ChangePasswordRequestDto model)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
                throw new KeyNotFoundException($"User with ID {id} was not found.");

            var result = await _userManager.ChangePasswordAsync(user, model.CurrentPassword, model.NewPassword);
            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                throw new InvalidOperationException(errors);
            }

            return true;
        }

        public async Task<bool> ResetPasswordAsync(string email, string token, string newPassword)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null) return false;

            var result = await _userManager.ResetPasswordAsync(user, token, newPassword);
            return result.Succeeded;
        }
    }
}