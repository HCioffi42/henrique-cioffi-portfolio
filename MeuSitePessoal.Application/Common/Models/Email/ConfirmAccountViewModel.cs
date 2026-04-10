namespace MeuSitePessoal.Application.Common.Models.Email;

/// <summary>
/// Model for the account confirmation email.
/// </summary>
public record ConfirmAccountViewModel
{
    /// <summary>
    /// Gets the user's name.
    /// </summary>
    public string UserName { get; init; } = string.Empty;

    /// <summary>
    /// Gets the confirmation link.
    /// </summary>
    public string ConfirmLink { get; init; } = string.Empty;
}
