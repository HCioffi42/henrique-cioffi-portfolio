namespace MeuSitePessoal.Application.Common.Interfaces;

/// <summary>
/// Provides a mechanism for sending email messages.
/// </summary>
public interface IEmailSender
{
    /// <summary>
    /// Sends an email message asynchronously to a single recipient.
    /// </summary>
    /// <param name="to">The recipient's email address.</param>
    /// <param name="subject">The subject of the email.</param>
    /// <param name="body">The HTML or plain text body of the email.</param>
    /// <param name="ct">A token to monitor for cancellation requests.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    Task SendEmailAsync(string to, string subject, string body, CancellationToken ct = default);
}
