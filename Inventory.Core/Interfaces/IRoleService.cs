using System.Collections.Generic;
using System.Threading.Tasks;
using Inventory.Core.DTOs;

namespace Inventory.Core.Interfaces
{
    public interface IRoleService
    {
        Task<IReadOnlyList<RoleResponseDto>> GetAllRolesAsync();
        Task<RoleResponseDto?> GetRoleByIdAsync(int id);
        Task<RoleResponseDto> CreateRoleAsync(RoleRequestDto model);
        Task<bool> UpdateRoleAsync(int id, RoleRequestDto model);
        Task<bool> DeleteRoleAsync(int id);
    }
}