using Microsoft.AspNetCore.Identity;

namespace MeuSitePessoal.Domain.Entities;

/// <summary>
/// Represents a customized Identity user for the platform.
/// Extends the base IdentityUser to include localized preferences.
/// </summary>
public class ApplicationUser : IdentityUser
{
    /// <summary>
    /// Gets or sets the user's preferred language for content and notifications.
    /// Defaults to 'en' (English).
    /// </summary>
    public string PreferredLanguage { get; set; } = "en";
}
