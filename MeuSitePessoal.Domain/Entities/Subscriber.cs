namespace MeuSitePessoal.Domain.Entities;

/// <summary>
/// Represents a user subscribed to the platform's anonymous newsletter.
/// This entity encapsulates its own logic for token generation and initial state, 
/// ensuring consistency across the application lifecycle.
/// </summary>
public class Subscriber
{
    /// <summary>
    /// Private parameterless constructor required by EF Core for entity materialization.
    /// It ensures that even when loaded from the database, the object maintains its structural integrity.
    /// </summary>
    private Subscriber() 
    {
        // Internal initialization logic if needed during materialization
    }
    
    /// <summary>
    /// Public constructor for creating new subscribers. 
    /// Enforces the requirement of an email and sets the initial secure state.
    /// </summary>
    /// <param name="email">The verified email address of the subscriber.</param>
    public Subscriber(string email)
    {
        Id = Guid.NewGuid();
        Email = email;
        SubscribedAt = DateTime.UtcNow;
        IsActive = true;
        IsVerified = false;
        
        // Generates secure, hyphen-free tokens for verification and unsubscription.
        VerificationToken = Guid.NewGuid().ToString("N");
        UnsubscribeToken = Guid.NewGuid().ToString("N");
    }
    
    /// <summary>
    /// Gets the unique identifier for the subscriber. Private set prevents accidental ID changes.
    /// </summary>
    public Guid Id { get; private set; }

    /// <summary>
    /// Gets the individual's email address. Private set ensures the email remains immutable after creation.
    /// </summary>
    public string Email { get; private set; } = string.Empty;

    /// <summary>
    /// Gets the timestamp of the subscription.
    /// </summary>
    public DateTime SubscribedAt { get; private set; }

    /// <summary>
    /// Gets or sets whether the subscriber is active. Public set allowed for administrative/user toggling.
    /// </summary>
    public bool IsActive { get; set; }

    /// <summary>
    /// Gets or sets whether the email is verified. 
    /// </summary>
    public bool IsVerified { get; set; }

    /// <summary>
    /// Gets the token for email verification. Private set protects the security token from external tampering.
    /// </summary>
    public string? VerificationToken { get; private set; }

    /// <summary>
    /// Gets the token for unsubscription. Private set ensures the token is managed only by the domain.
    /// </summary>
    public string? UnsubscribeToken { get; private set; }

    /// <summary>
    /// Gets or sets the timestamp of verification.
    /// </summary>
    public DateTime? VerifiedAt { get; set; }
    
    // HC: Domain method to handle the verification logic internally.
    public void ConfirmVerification(string token)
    {
        if (VerificationToken != token) throw new InvalidOperationException("Invalid token.");
        
        IsVerified = true;
        IsActive = true;
        VerifiedAt = DateTime.UtcNow;
        VerificationToken = null; // HC: Now possible because we are inside the class.
    }

    // HC: Method to allow token refresh (used in the Subscribe flow).
    public void UpdateVerificationToken(string newToken)
    {
        VerificationToken = newToken;
    }
}