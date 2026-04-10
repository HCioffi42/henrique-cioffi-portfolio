namespace MeuSitePessoal.Application.Common.Models.Email;

/// <summary>
/// Model for the password reset email.
/// </summary>
public record ResetPasswordViewModel
{
    /// <summary>
    /// Gets the user's name.
    /// </summary>
    public string UserName { get; init; } = string.Empty;

    /// <summary>
    /// Gets the password reset link.
    /// </summary>
    public string ResetLink { get; init; } = string.Empty;
}
