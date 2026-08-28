using System.Globalization;
using System.Security.Claims;

namespace Inventory.Web.Extensions
{
    public static class ClaimsPrincipalExtensions
    {
        public static string GetUserId(this ClaimsPrincipal user) =>
            user.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0";

        public static string GetUserEmail(this ClaimsPrincipal user) =>
            user.FindFirstValue(ClaimTypes.Email) ?? string.Empty;

        public static string GetDisplayName(this ClaimsPrincipal user)
        {
            var isRtl = CultureInfo.CurrentUICulture.TextInfo.IsRightToLeft;
            var nameAr = user.FindFirst("NameAr")?.Value;
            var nameEn = user.FindFirst("NameEn")?.Value;

            if (isRtl)
            {
                if (!string.IsNullOrWhiteSpace(nameAr)) return nameAr;
                if (!string.IsNullOrWhiteSpace(user.Identity?.Name)) return user.Identity.Name;
                return "مستخدم";
            }

            if (!string.IsNullOrWhiteSpace(nameEn)) return nameEn;
            if (!string.IsNullOrWhiteSpace(user.Identity?.Name)) return user.Identity.Name;
            return "User";
        }

        public static string GetUserInitial(this ClaimsPrincipal user)
        {
            var name = user.GetDisplayName();
            return !string.IsNullOrWhiteSpace(name)
                ? name.Trim().Substring(0, 1).ToUpper()
                : "U";
        }

        public static bool IsSuperAdmin(this ClaimsPrincipal user) =>
            user.IsInRole("SuperAdmin") || user.IsInRole("Super Admin");
    }
}