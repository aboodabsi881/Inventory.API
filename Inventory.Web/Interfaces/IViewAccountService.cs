using Inventory.Web.ViewModels.Accounts;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace Inventory.Web.Interfaces
{
    public interface IViewAccountService
    {
        Task<List<UsersVM>> GetAllUsersAsync();
        Task<(bool Success, UsersVM? User, string Message)> LoginAsync(LoginVM model);
        Task<(bool Success, string Message)> RegisterAsync(RegisterVM model);
        Task<PersonalDataVM?> GetPersonalDataAsync(int id);
        Task<(bool Success, string Message)> UpdatePersonalDataAsync(PersonalDataVM model);
        Task<(bool Success, string Message)> ChangePasswordAsync(ChangePasswordVM model);
        Task<(bool Success, string Message)> DeleteUserAsync(int id);
        Task<string> SaveUserImageAsync(IFormFile imgFile);
        ClaimsPrincipal CreateClaimsPrincipal(UsersVM userObj);
    }
}