using Microsoft.Extensions.Configuration;

namespace MeuSitePessoal.Tests.Integration;

/// <summary>
/// Resolves the connection string for the dedicated integration-test database.
/// The suite drops and re-creates whatever database this points at, so it must never
/// address the development or production instance.
/// </summary>
/// <remarks>
/// The value is configuration rather than a constant so that the same assembly runs against a
/// developer's local server and against CI's service container without a code change, and so
/// that no database credential is carried in source. Environment variables are applied last and
/// therefore win, which is what lets CI override a developer's stored value.
/// </remarks>
internal static class TestDatabase
{
    private const string ConnectionStringKey = "ConnectionStrings:DefaultConnection";

    private static readonly Lazy<string> Lazy = new(Resolve);

    /// <summary>
    /// The resolved connection string. Resolution happens once per test run.
    /// </summary>
    internal static string ConnectionString => Lazy.Value;

    private static string Resolve()
    {
        var configuration = new ConfigurationBuilder()
            .AddUserSecrets(typeof(TestDatabase).Assembly, optional: true)
            .AddEnvironmentVariables()
            .Build();

        var connectionString = configuration[ConnectionStringKey];

        if (!string.IsNullOrWhiteSpace(connectionString))
        {
            return connectionString;
        }

        // Fails with the remedy rather than with a connection error forty frames deep, because
        // the first symptom a developer sees is otherwise every integration test failing at once.
        throw new InvalidOperationException(
            $"No integration-test database is configured. Set '{ConnectionStringKey}' with:" +
            Environment.NewLine +
            Environment.NewLine +
            "    dotnet user-secrets set \"ConnectionStrings:DefaultConnection\" " +
            "\"Host=localhost;Port=5432;Database=meusitepessoal_testdb;Username=postgres;Password=<your local password>\" " +
            "--project MeuSitePessoal.Tests" +
            Environment.NewLine +
            Environment.NewLine +
            "or supply it as the ConnectionStrings__DefaultConnection environment variable. " +
            "The suite drops and re-creates this database, so it must be one dedicated to testing.");
    }
}
