using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;

namespace Inventory.Web.Handlers
{
    public class UserAuthHeaderHandler : DelegatingHandler
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public UserAuthHeaderHandler(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var user = _httpContextAccessor.HttpContext?.User;

            if (user?.Identity?.IsAuthenticated == true)
            {
                var userId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value
                             ?? user.FindFirst("uid")?.Value
                             ?? user.FindFirst("sub")?.Value
                             ?? user.FindFirst("Id")?.Value;

                if (!string.IsNullOrEmpty(userId))
                {
                    request.Headers.Remove("X-User-Id");
                    request.Headers.Add("X-User-Id", userId);
                }

                var userName = user.Identity.Name;
                if (!string.IsNullOrEmpty(userName))
                {
                    request.Headers.Remove("X-User-Name");
                    request.Headers.Add("X-User-Name", userName);
                }
            }

            return await base.SendAsync(request, cancellationToken);
        }
    }
}