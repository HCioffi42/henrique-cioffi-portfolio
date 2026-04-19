using MeuSitePessoal.Application.Common.Interfaces;
using Microsoft.AspNetCore.Http;

namespace MeuSitePessoal.Infrastructure.Services;

/// <summary>
/// Implements the ILanguageProvider by inspecting the Accept-Language header or falling back to default.
/// </summary>
public class HttpContextLanguageProvider : ILanguageProvider
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public HttpContextLanguageProvider(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public string GetCurrentLanguage()
    {
        var context = _httpContextAccessor.HttpContext;
        if (context == null) return "en";

        // HC: In the future, we can add logic here to check if the user is authenticated 
        // and return ApplicationUser.PreferredLanguage.
        
        var acceptLanguage = context.Request.Headers["Accept-Language"].ToString();
        
        if (string.IsNullOrWhiteSpace(acceptLanguage)) return "en";

        // Simplistic check for Portuguese
        if (acceptLanguage.Contains("pt", StringComparison.OrdinalIgnoreCase))
        {
            return "pt";
        }

        return "en";
    }
}
