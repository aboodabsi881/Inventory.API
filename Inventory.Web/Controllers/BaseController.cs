using Inventory.Web.ViewModels.Accounts;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;

namespace Inventory.Web.Controllers
{
    public class BaseController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public BaseController(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public override async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            // Fetch logged-in user info (or active user) for the navbar on every request
            try
            {
                var client = _httpClientFactory.CreateClient("InventoryAPI");
                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                var users = await client.GetFromJsonAsync<List<UsersVM>>("Accounts/users", options);

                var activeUser = users?.FirstOrDefault();
                if (activeUser != null)
                {
                    ViewData["UserAvatar"] = !string.IsNullOrWhiteSpace(activeUser.Img) ? activeUser.Img : "/images/Portrait_Placeholder.png";
                    ViewData["UserName"] = !string.IsNullOrWhiteSpace(activeUser.UserName) ? activeUser.UserName : "User";
                }
            }
            catch
            {
                ViewData["UserAvatar"] = "/images/Portrait_Placeholder.png";
                ViewData["UserName"] = "User";
            }

            await base.OnActionExecutionAsync(context, next);
        }
    }
}