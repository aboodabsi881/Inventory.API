using AutoMapper;
using Inventory.Core.DTOs;
using Inventory.Core.Entities.Users.ApplicationRoles;
using Inventory.Core.Entities.Users.ApplicationUsers;
using Inventory.Core.Interfaces;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Inventory.Core.Services
{
    public class AccountService : IAccountService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<ApplicationRole> _roleManager;
        private readonly IMapper _mapper;
        private readonly IWebHostEnvironment _env;

        public AccountService(
            UserManager<ApplicationUser> userManager,
            RoleManager<ApplicationRole> roleManager,
            IMapper mapper,
            IWebHostEnvironment env)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _mapper = mapper;
            _env = env;
        }

        public async Task<UserResponseDto> GetUserByIdAsync(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
                throw new KeyNotFoundException($"User with ID {id} was not found.");

            var roles = await _userManager.GetRolesAsync(user);
            var response = _mapper.Map<UserResponseDto>(user);
            response.Roles = roles.ToList();

            return response;
        }

        public async Task<UserResponseDto> GetUserByEmailAsync(string email)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
                throw new KeyNotFoundException($"User with email {email} was not found.");

            var roles = await _userManager.GetRolesAsync(user);
            var response = _mapper.Map<UserResponseDto>(user);
            response.Roles = roles.ToList();

            return response;
        }

        public async Task<IReadOnlyList<UserResponseDto>> GetAllUsersAsync()
        {
            var users = await _userManager.Users.ToListAsync();
            var userList = new List<UserResponseDto>();

            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);
                var dto = _mapper.Map<UserResponseDto>(user);
                dto.Roles = roles.ToList();
                userList.Add(dto);
            }

            return userList;
        }

        public async Task<UserResponseDto?> SignInUserAsync(LoginRequestDto model)
        {
            var user = model.UserName.Contains("@")
                ? await _userManager.FindByEmailAsync(model.UserName)
                : await _userManager.FindByNameAsync(model.UserName);

            if (user == null || !await _userManager.CheckPasswordAsync(user, model.Password))
                return null;

            var roles = await _userManager.GetRolesAsync(user);
            var response = _mapper.Map<UserResponseDto>(user);
            response.Roles = roles.ToList();

            return response;
        }

        public async Task<UserResponseDto> CreateUserAsync(RegisterRequestDto model)
        {
            var user = _mapper.Map<ApplicationUser>(model);
            user.PasswordByte = Encoding.UTF8.GetBytes(model.Password);

            // Process optional Base64 Image string
            if (!string.IsNullOrEmpty(model.ImgBase64))
            {
                try
                {
                    var base64Data = model.ImgBase64.Contains(",")
                        ? model.ImgBase64.Split(',')[1]
                        : model.ImgBase64;

                    byte[] imageBytes = Convert.FromBase64String(base64Data);
                    var fileName = $"{Guid.NewGuid()}.png";
                    var directoryPath = Path.Combine(_env.WebRootPath, "uploads", "users");

                    if (!Directory.Exists(directoryPath))
                        Directory.CreateDirectory(directoryPath);

                    var fullPath = Path.Combine(directoryPath, fileName);
                    await File.WriteAllBytesAsync(fullPath, imageBytes);

                    user.Img = "/uploads/users/" + fileName;
                }
                catch
                {
                    user.Img = "/img/blank-profile-picture-973460_1280.png";
                }
            }
            else
            {
                user.Img = "/img/blank-profile-picture-973460_1280.png";
            }

            var result = await _userManager.CreateAsync(user, model.Password);
            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                throw new InvalidOperationException($"User creation failed: {errors}");
            }

            // Assign baseline role
            var roleName = string.IsNullOrEmpty(model.RoleName) ? "User" : model.RoleName;
            if (await _roleManager.RoleExistsAsync(roleName))
            {
                await _userManager.AddToRoleAsync(user, roleName);
            }

            var response = _mapper.Map<UserResponseDto>(user);
            response.Roles = new List<string> { roleName };

            return response;
        }

        public async Task<bool> AddUserToRoleAsync(string userId, string roleName)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return false;

            var result = await _userManager.AddToRoleAsync(user, roleName);
            return result.Succeeded;
        }

        public async Task<PersonalDataResponseDto> GetPersonalDataAsync(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
                throw new KeyNotFoundException($"User with ID {id} was not found.");

            var roles = await _userManager.GetRolesAsync(user);
            var response = _mapper.Map<PersonalDataResponseDto>(user);
            response.RoleName = roles.FirstOrDefault() ?? "User";

            return response;
        }

        public async Task<PersonalDataResponseDto> UpdateUserAsync(string id, PersonalDataRequestDto model, IFormFile? imgFile = null)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
                throw new KeyNotFoundException($"User with ID {id} was not found.");

            _mapper.Map(model, user);

            if (imgFile != null && imgFile.Length > 0)
            {
                user.Img = await SaveUserImageAsync(imgFile);
            }
            else if (string.IsNullOrEmpty(user.Img))
            {
                user.Img = "/img/blank-profile-picture-973460_1280.png";
            }

            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                throw new InvalidOperationException($"Failed to update user: {errors}");
            }

            if (!string.IsNullOrEmpty(model.RoleName))
            {
                var currentRoles = await _userManager.GetRolesAsync(user);
                if (currentRoles.Any())
                    await _userManager.RemoveFromRolesAsync(user, currentRoles);

                await _userManager.AddToRoleAsync(user, model.RoleName);
            }

            var response = _mapper.Map<PersonalDataResponseDto>(user);
            response.RoleName = model.RoleName;

            return response;
        }

        public async Task<string> UpdateUserImgAsync(string id, IFormFile imgFile)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
                throw new KeyNotFoundException($"User with ID {id} was not found.");

            if (imgFile != null && imgFile.Length > 0)
            {
                user.Img = await SaveUserImageAsync(imgFile);
                var result = await _userManager.UpdateAsync(user);

                if (!result.Succeeded)
                {
                    var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                    throw new InvalidOperationException($"Failed to update profile image: {errors}");
                }
            }

            return user.Img ?? "/img/blank-profile-picture-973460_1280.png";
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
                throw new InvalidOperationException($"Password change failed: {errors}");
            }

            return true;
        }

        public async Task<bool> ResetPasswordAsync(string email, string token, string newPassword)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
                throw new KeyNotFoundException($"User with email {email} was not found.");

            var result = await _userManager.ResetPasswordAsync(user, token, newPassword);
            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                throw new InvalidOperationException($"Reset password failed: {errors}");
            }

            return true;
        }

        public async Task<bool> DeleteUserAsync(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
                throw new KeyNotFoundException($"User with ID {id} was not found.");

            var result = await _userManager.DeleteAsync(user);
            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                throw new InvalidOperationException($"User deletion failed: {errors}");
            }

            return true;
        }

        private async Task<string> SaveUserImageAsync(IFormFile imgFile)
        {
            var fileName = $"{Guid.NewGuid()}{Path.GetExtension(imgFile.FileName)}";
            var directoryPath = Path.Combine(_env.WebRootPath, "uploads", "users");

            if (!Directory.Exists(directoryPath))
                Directory.CreateDirectory(directoryPath);

            var fullPath = Path.Combine(directoryPath, fileName);

            await using (var stream = new FileStream(fullPath, FileMode.Create))
            {
                await imgFile.CopyToAsync(stream);
            }

            return "/uploads/users/" + fileName;
        }
    }
}