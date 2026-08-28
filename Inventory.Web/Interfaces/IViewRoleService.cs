using Inventory.Web.ViewModels.Roles;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Inventory.Web.Interfaces
{
    public interface IViewRoleService
    {
        Task<List<RoleVM>> GetAllRolesAsync();
        Task<RoleVM?> GetRoleByIdAsync(int id);
        Task<(bool Success, string Message)> CreateRoleAsync(RoleVM roleVM);
        Task<(bool Success, string Message)> UpdateRoleAsync(int id, RoleVM roleVM);
        Task<(bool Success, bool IsProtected, string Message)> DeleteRoleAsync(int id);
    }
}