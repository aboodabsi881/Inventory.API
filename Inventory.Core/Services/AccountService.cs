using Inventory.Core.DTOs;
using Inventory.Core.Entities.Users.ApplicationRoles;
using Inventory.Core.Entities.Users.ApplicationUsers;
using Inventory.Core.Interfaces;
using Microsoft.AspNetCore.Identity;

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

        public async Task<UserResponseDto?> SignInUserAsync(LoginRequestDto model)
        {
            var user = await _appUserRepo.GetFirstOrDefaultAsync(u => u.Email == model.Username || u.UserName == model.Username);
            if (user == null) return null;

            var dto = await _appUserRepo.GetDtoByIdAsync<UserResponseDto>(user.Id);
            if (dto != null)
            {
                var roles = await GetRolesForUserAsync(user.Id);
                dto.Roles = roles;
                dto.RoleName = roles.FirstOrDefault() ?? "User"; 


            }

            return dto;
        }

        public async Task<UserResponseDto> CreateUserAsync(RegisterRequestDto model)
        {
            var existingUser = await _appUserRepo.GetFirstOrDefaultAsync(u =>
                u.Email == model.Email || (!string.IsNullOrEmpty(model.Username) && u.UserName == model.Username));

            if (existingUser != null)
            {
                if (existingUser.Email == model.Email)
                    throw new InvalidOperationException($"The email '{model.Email}' is already in use.");

                throw new InvalidOperationException($"The username '{model.Username}' is already taken.");
            }

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
            var users = await _appUserRepo.GetAllDtoAsync<UserResponseDto>();

            foreach (var user in users)
            {
                var roles = await GetRolesForUserAsync(user.Id);

                user.Roles = roles;
                user.RoleName = roles.FirstOrDefault() ?? "User";
            }

            return users;
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

        public async Task<bool> ChangePasswordAsync(string id, ChangePasswordRequestDto model)
        {
            return await Task.FromResult(true);
        }

        public async Task<bool> ResetPasswordAsync(string email, string token, string newPassword)
        {
            return await Task.FromResult(true);
        }

        private async Task<List<string>> GetRolesForUserAsync(int userId)
        {
            var userRoles = (await _userRoleRepo.GetAllAsync())
                                                .Where(ur => ur.UserId == userId)
                                                    .Select(ur => ur.RoleId).ToList();


            var roles = (await _roleRepo.GetAllAsync()).
                                        Where(r => userRoles.Contains(r.Id))
                                                        .Select(r => r.Name ?? string.Empty)
                                                                                        .ToList();
            return roles;
        }

        private async Task<bool> AddUserToRoleByNameAsync(int userId, string roleName)
        {
            var role = (await _roleRepo.GetAllAsync())
                                .FirstOrDefault(r => r.Name == roleName);

            if (role == null)
            {
                return false;
            }

            await _userRoleRepo.AddAsync(new IdentityUserRole<int>
            {
                UserId = userId,
                RoleId = role.Id
            });
            return await _userRoleRepo.SaveChangesAsync() > 0;
        }
    }
}