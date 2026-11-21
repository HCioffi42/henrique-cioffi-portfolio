using MeuSitePessoal.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Npgsql;
using Respawn;
using Respawn.Graph;

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

    private static readonly SemaphoreSlim Gate = new(1, 1);

    private static Respawner? _respawner;

    /// <summary>
    /// Returns the database to an empty state for the test about to run.
    /// </summary>
    /// <remarks>
    /// The schema is built once per test run and then kept. Every subsequent test clears the
    /// data with Respawn, which deletes rows in foreign-key order and is a fraction of the cost
    /// of dropping the database and replaying every migration, as this fixture previously did
    /// before each individual test (audit T2).
    ///
    /// The guard is held across the whole operation so that enabling parallel execution cannot
    /// have one test truncating the database while another is arranging its rows. It does not by
    /// itself make the suite safe to parallelize: the tests still share a single database, so
    /// <c>DisableTestParallelization</c> remains load-bearing.
    /// </remarks>
    internal static async Task ResetAsync(BlogDbContext context)
    {
        await Gate.WaitAsync();

        try
        {
            if (_respawner is null)
            {
                // Drops first, so a schema left behind by another branch cannot survive into
                // this run and present itself as a migration that has already been applied.
                await context.Database.EnsureDeletedAsync();
                await context.Database.MigrateAsync();

                await using var created = new NpgsqlConnection(ConnectionString);
                await created.OpenAsync();
                _respawner = await Respawner.CreateAsync(created, BuildOptions());

                // The database was just built, so there is nothing to clear.
                return;
            }

            await using var connection = new NpgsqlConnection(ConnectionString);
            await connection.OpenAsync();
            await _respawner.ResetAsync(connection);
        }
        finally
        {
            Gate.Release();
        }
    }

    private static RespawnerOptions BuildOptions() => new()
    {
        DbAdapter = DbAdapter.Postgres,
        SchemasToInclude = ["public"],

        // Clearing the migration history would make the schema read as unmigrated, and the next
        // run would replay every migration against tables that already exist.
        TablesToIgnore = [new Table("__EFMigrationsHistory")]
    };

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
