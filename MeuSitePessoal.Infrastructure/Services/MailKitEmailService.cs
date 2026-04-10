using MailKit.Net.Smtp;
using MailKit.Security;
using MeuSitePessoal.Application.Common.Interfaces;
using MeuSitePessoal.Infrastructure.Configuration;
using Microsoft.Extensions.Options;
using MimeKit;

namespace MeuSitePessoal.Infrastructure.Services;

/// <summary>
/// Provides an implementation of the <see cref="IEmailSender"/> using the MailKit library.
/// </summary>
public class MailKitEmailService : IEmailSender
{
    private readonly EmailSettings _settings;

    /// <summary>
    /// Initializes a new instance of the <see cref="MailKitEmailService"/> class with the specified settings.
    /// </summary>
    /// <param name="settings">The SMTP configuration settings injected via IOptions.</param>
    public MailKitEmailService(IOptions<EmailSettings> settings)
    {
        _settings = settings.Value;
    }

    /// <summary>
    /// Sends an email message asynchronously using the configured SMTP server and MailKit.
    /// </summary>
    /// <param name="to">The recipient's email address.</param>
    /// <param name="subject">The subject of the email.</param>
    /// <param name="body">The HTML body of the email.</param>
    /// <param name="ct">A token to monitor for cancellation requests.</param>
    /// <returns>A task that represents the asynchronous send operation.</returns>
    public async Task SendEmailAsync(string to, string subject, string body, CancellationToken ct = default)
    {
        var message = CreateMessage(to, subject, body);

        using var client = new SmtpClient();

        // The service uses SecureSocketOptions.Auto if EnableSsl is true to support StartTLS and SSL/TLS.
        var socketOptions = _settings.EnableSsl ? SecureSocketOptions.Auto : SecureSocketOptions.None;

        await client.ConnectAsync(_settings.Host, _settings.Port, socketOptions, ct);
        await client.AuthenticateAsync(_settings.Username, _settings.Password, ct);
        await client.SendAsync(message, ct);
        await client.DisconnectAsync(true, ct);
    }

    /// <summary>
    /// Constructs the email message with the configured sender and recipients.
    /// </summary>
    /// <param name="to">The recipient email address.</param>
    /// <param name="subject">The email subject.</param>
    /// <param name="body">The email body.</param>
    /// <returns>A configured <see cref="MimeMessage"/> instance.</returns>
    internal MimeMessage CreateMessage(string to, string subject, string body)
    {
        var message = new MimeMessage();
        
        message.From.Add(new MailboxAddress(_settings.SenderName, _settings.SenderEmail));
        message.To.Add(MailboxAddress.Parse(to));
        message.Subject = subject;

        var builder = new BodyBuilder
        {
            HtmlBody = body
        };

        message.Body = builder.ToMessageBody();

        return message;
    }
}

