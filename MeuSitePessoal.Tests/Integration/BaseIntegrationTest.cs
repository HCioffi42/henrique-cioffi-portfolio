using MeuSitePessoal.Api;
using MeuSitePessoal.Infrastructure.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace MeuSitePessoal.Tests.Integration;

public class BaseIntegrationTest : IAsyncLifetime
{
    protected WebApplicationFactory<Program> _factory = default!;
    protected HttpClient _client = default!;

    // Hardcodes the connection string for a local PostgreSQL instance used exclusively for integration tests.
    private const string TestConnectionString = "Host=localhost;Port=5432;Database=meusitepessoal_testdb;Username=postgres;Password=Anah@1418";

    public async Task InitializeAsync()
    {
        _factory = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                // Configures the environment to bypass development-only startup logic (like database seeding).
                builder.UseEnvironment("Testing");
                
                builder.ConfigureServices(services =>
                {
                    // Removes the existing DbContext registration to ensure the main application database remains untouched.
                    var descriptor = services.SingleOrDefault(
                        d => d.ServiceType == typeof(DbContextOptions<BlogDbContext>));

                    if (descriptor != null)
                    {
                        services.Remove(descriptor);
                    }

                    // Injects the DbContext configured to use the dedicated local test database.
                    services.AddDbContext<BlogDbContext>(options =>
                    {
                        options.UseNpgsql(TestConnectionString);
                    });
                });
            });

        _client = _factory.CreateClient();
        
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<BlogDbContext>();
        
        // Deletes and recreates the database from scratch to guarantee a clean state for every test execution.
        await db.Database.EnsureDeletedAsync();
        await db.Database.MigrateAsync();
    }

    public async Task DisposeAsync()
    {
        await _factory.DisposeAsync();
    }
}