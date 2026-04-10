namespace MeuSitePessoal.Infrastructure.Configuration;

/// <summary>
/// Represents the SMTP configuration settings required for email delivery.
/// </summary>
public record EmailSettings
{
    /// <summary>
    /// Gets the section name for binding configuration.
    /// </summary>
    public const string SectionName = "EmailSettings";

    /// <summary>
    /// Gets or sets the SMTP server host address.
    /// </summary>
    public string Host { get; init; } = string.Empty;

    /// <summary>
    /// Gets or sets the SMTP server port.
    /// </summary>
    public int Port { get; init; }

    /// <summary>
    /// Gets or sets the username for SMTP authentication.
    /// </summary>
    public string Username { get; init; } = string.Empty;

    /// <summary>
    /// Gets or sets the password for SMTP authentication.
    /// </summary>
    public string Password { get; init; } = string.Empty;

    /// <summary>
    /// Gets or sets the email address to be used as the sender.
    /// </summary>
    public string SenderEmail { get; init; } = string.Empty;

    /// <summary>
    /// Gets or sets the name to be displayed as the sender.
    /// </summary>
    public string SenderName { get; init; } = string.Empty;

    /// <summary>
    /// Gets or sets a value indicating whether SSL/TLS should be enabled for the connection.
    /// </summary>
    public bool EnableSsl { get; init; } = true;
}
