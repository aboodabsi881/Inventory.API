using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;

namespace Inventory.Web.Controllers
{
    public class LanguageController : Controller
    {
        [HttpGet]
        public IActionResult ChangeLanguage(string lang, string returnUrl)
        {
            Response.Cookies.Append(
                CookieRequestCultureProvider.DefaultCookieName,
                CookieRequestCultureProvider.MakeCookieValue(new RequestCulture(lang)),
                new CookieOptions
                {
                    Expires = DateTimeOffset.UtcNow.AddYears(1),
                    IsEssential = true,
                    SameSite = SameSiteMode.Lax
                }
            );

            return LocalRedirect(string.IsNullOrEmpty(returnUrl) ? "~/" : returnUrl);
        }
    }
}