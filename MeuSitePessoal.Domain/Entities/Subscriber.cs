namespace MeuSitePessoal.Domain.Entities;

/// <summary>
/// Represents a user subscribed to the platform's anonymous newsletter.
/// </summary>
public class Subscriber
{
    /// <summary>
    /// Gets or sets the unique identifier for the subscriber.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Gets or sets the individual's email address. Must be unique in the persistence store.
    /// </summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the timestamp indicating when the user successfully subscribed.
    /// </summary>
    public DateTime SubscribedAt { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the subscriber actively seeks to receive newsletters.
    /// </summary>
    public bool IsActive { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the email address has been verified through a double opt-in link.
    /// </summary>
    public bool IsVerified { get; set; }

    /// <summary>
    /// Gets or sets the unique token used for verifying the subscription via email.
    /// </summary>
    public string? VerificationToken { get; set; }

    /// <summary>
    /// Gets or sets the timestamp indicating when the subscriber was verified.
    /// </summary>
    public DateTime? VerifiedAt { get; set; }
}

