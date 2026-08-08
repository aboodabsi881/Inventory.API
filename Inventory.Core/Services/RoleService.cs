using Inventory.Core.DTOs;
using Inventory.Core.Entities.Users.ApplicationRoles;
using Inventory.Core.Interfaces;

namespace Inventory.Core.Services
{
    public class RoleService : IRoleService 
    {
        private readonly IRepository<ApplicationRole> _roleRepo;

        public RoleService(IRepository<ApplicationRole> roleRepo)
        {
            _roleRepo = roleRepo;
        }

        public async Task<IReadOnlyList<RoleResponseDto>> GetAllRolesAsync()
        {
            return await _roleRepo.GetAllDtoAsync<RoleResponseDto>();
        }

        public async Task<RoleResponseDto?> GetRoleByIdAsync(int id)
        {
            return await _roleRepo.GetDtoByIdAsync<RoleResponseDto>(id);
        }

        public async Task<RoleResponseDto> CreateRoleAsync(RoleRequestDto model)
        {
            return await _roleRepo.CreateFromDtoAsync<RoleRequestDto, RoleResponseDto>(model);
        }

        public async Task<bool> UpdateRoleAsync(int id, RoleRequestDto model)
        {
            var result = await _roleRepo.UpdateFromDtoAsync<RoleRequestDto, RoleResponseDto>(id, model);
            return result != null;
        }

        public async Task<bool> DeleteRoleAsync(int id)
        {
            return await _roleRepo.DeleteAndSaveAsync(id);
        }
    }
}
