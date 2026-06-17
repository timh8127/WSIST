using System.Security.Claims;
using Microsoft.AspNetCore.Localization;
using WSIST.Engine;

namespace WSIST.Web;

// Resolves the UI language for a signed-in user from their stored
// PreferredLanguage. Runs ahead of the cookie and Accept-Language providers, so
// a saved preference wins and persists across logout/login (and across devices).
// While PreferredLanguage is null this returns null and the next provider
// (cookie, then Accept-Language) decides — that's how a first-time user defaults
// to their browser language.
public sealed class DbRequestCultureProvider : RequestCultureProvider
{
    public override async Task<ProviderCultureResult?> DetermineProviderCultureResult(
        HttpContext httpContext
    )
    {
        if (httpContext.User?.Identity?.IsAuthenticated != true)
            return null;

        var email = httpContext.User.FindFirst(ClaimTypes.Email)?.Value;
        if (string.IsNullOrEmpty(email))
            return null;

        var management = httpContext.RequestServices.GetService<TestManagement>();
        if (management is null)
            return null;

        var lang = await management.GetPreferredLanguageByEmailAsync(
            email,
            httpContext.RequestAborted
        );
        if (string.IsNullOrEmpty(lang))
            return null;

        return new ProviderCultureResult(lang);
    }
}
