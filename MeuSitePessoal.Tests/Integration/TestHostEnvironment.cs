namespace MeuSitePessoal.Tests.Integration;

/// <summary>
/// Prepares the process environment that every integration test host is built from.
/// Both settings must be in place before the first host is constructed, because startup reads
/// them while the application builder is being configured.
/// </summary>
internal static class TestHostEnvironment
{
    /// <summary>
    /// Signs tokens that are issued and consumed entirely within the test process.
    /// This is not a credential and is deliberately recognisable as a test value.
    /// </summary>
    internal const string SigningKey = "integration-test-signing-key-not-a-production-secret";

    /// <summary>
    /// Applies the environment the test hosts require. Idempotent, so every fixture that builds a
    /// host may call it without depending on which test class happens to run first.
    /// </summary>
    internal static void Apply()
    {
        // Declares the environment explicitly. Startup loads user secrets when this variable is
        // absent, which it otherwise is under 'dotnet test'. Those secrets rank above environment
        // variables, so a developer's local signing key would be used to validate tokens while
        // the fixture's key signed them, and every authenticated test would fail with 401.
        // Naming the environment keeps the suite hermetic from whatever is configured locally.
        Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "Testing");

        // Supplies the signing key through the environment, which is the only configuration
        // source a build-time read observes: WebApplicationFactory merges its own in-memory
        // sources at Build(), after startup has already resolved the key.
        Environment.SetEnvironmentVariable("JwtSettings__Key", SigningKey);
    }
}
