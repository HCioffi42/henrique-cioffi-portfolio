namespace MeuSitePessoal.Application.Common.Models.Email;

/// <summary>
/// Model for the newsletter subscription verification email.
/// </summary>
public record NewsletterVerificationViewModel
{
    /// <summary>
    /// Gets the verification link.
    /// </summary>
    public string ConfirmLink { get; init; } = string.Empty;
}
