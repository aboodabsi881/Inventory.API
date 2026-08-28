using Inventory.Core.Interfaces;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace Inventory.Core.Services
{
    public class CurrentUserService : ICurrentUserService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public CurrentUserService(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public int? UserId
        {
            get
            {
                var context = _httpContextAccessor.HttpContext;
                if (context == null) return null;

                var claimVal = context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value
                               ?? context.User?.FindFirst("sub")?.Value
                               ?? context.User?.FindFirst("uid")?.Value;

                if (int.TryParse(claimVal, out int idFromClaim))
                    return idFromClaim;

                if (context.Request.Headers.TryGetValue("X-User-Id", out var headerVal) &&
                    int.TryParse(headerVal, out int idFromHeader))
                {
                    return idFromHeader;
                }

                return null;
            }
        }

        public string? UserName
        {
            get
            {
                var context = _httpContextAccessor.HttpContext;
                if (context == null) return null;

                return context.User?.Identity?.Name
                    ?? (context.Request.Headers.TryGetValue("X-User-Name", out var nameVal) ? nameVal.ToString() : null);
            }
        }
    }
}