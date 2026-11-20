namespace MeuSitePessoal.Api.Configuration;

/// <summary>
/// The request budget for a single endpoint policy.
/// </summary>
public sealed class RateLimitPolicyOptions
{
    /// <summary>How many requests a single client may make within the window.</summary>
    public int PermitLimit { get; init; }

    /// <summary>The length of the fixed window, in minutes.</summary>
    public int WindowMinutes { get; init; }
}

/// <summary>
/// Request-throttling thresholds, bound from configuration so that each environment declares
/// its own. No threshold is expressed as a literal: a shared environment driving an automated
/// suite needs different numbers from production, and changing them must not require a rebuild.
/// </summary>
public sealed class RateLimitingOptions
{
    public const string SectionName = "RateLimiting";

    public Dictionary<string, RateLimitPolicyOptions> Policies { get; init; } = new();
}

/// <summary>
/// The policy names applied to endpoints. Endpoints reference these constants rather than
/// string literals, so a typo is a compile error rather than a silently unprotected endpoint.
/// </summary>
public static class RateLimitPolicies
{
    /// <summary>Password sign-in.</summary>
    public const string AuthSignIn = "auth-signin";

    /// <summary>
    /// Verification of a secret the caller supplies rather than a password: the two-factor code
    /// and the password-reset token. Deliberately separate from <see cref="AuthSignIn"/>, because
    /// a two-factor sign-in calls both and a shared budget would charge two permits per attempt.
    /// </summary>
    public const string AuthVerify = "auth-verify";

    /// <summary>Account creation.</summary>
    public const string AuthRegister = "auth-register";

    /// <summary>Password recovery, which dispatches an outbound message.</summary>
    public const string AuthRecovery = "auth-recovery";

    /// <summary>Comment creation.</summary>
    public const string CommentsWrite = "comments-write";

    /// <summary>Newsletter subscription, which dispatches an outbound message.</summary>
    public const string NewsletterSubscribe = "newsletter-subscribe";

    /// <summary>
    /// Every declared policy. Startup validates that each has a configured entry.
    /// </summary>
    public static readonly string[] All =
    [
        AuthSignIn,
        AuthVerify,
        AuthRegister,
        AuthRecovery,
        CommentsWrite,
        NewsletterSubscribe
    ];
}
