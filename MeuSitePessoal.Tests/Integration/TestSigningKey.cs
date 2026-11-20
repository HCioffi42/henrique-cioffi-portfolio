namespace MeuSitePessoal.Tests.Integration;

/// <summary>
/// Supplies the JWT signing key to test hosts.
/// The key is resolved by startup code that runs while the application builder is being
/// configured, which is earlier than the point at which WebApplicationFactory merges its own
/// in-memory configuration sources. An environment variable is read by the builder immediately,
/// so it is the only source a build-time read observes, and it exercises the same mechanism
/// deployed environments use.
/// </summary>
internal static class TestSigningKey
{
    /// <summary>
    /// Signs tokens that are issued and consumed entirely within the test process.
    /// This is not a credential and is deliberately recognisable as a test value.
    /// </summary>
    internal const string Value = "integration-test-signing-key-not-a-production-secret";

    /// <summary>
    /// Publishes the key to the process environment. Idempotent, so every fixture that builds a
    /// host may call it without depending on which test class happens to run first.
    /// </summary>
    internal static void PublishToEnvironment()
    {
        Environment.SetEnvironmentVariable("JwtSettings__Key", Value);
    }
}
