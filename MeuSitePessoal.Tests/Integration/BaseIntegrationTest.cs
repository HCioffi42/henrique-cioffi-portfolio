using System.Net.Http.Headers;
using System.Net.Http.Json;
using MeuSitePessoal.Api;
using MeuSitePessoal.Infrastructure.Configuration;
using MeuSitePessoal.Infrastructure.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace MeuSitePessoal.Tests.Integration;

public class BaseIntegrationTest : IAsyncLifetime
{
    protected WebApplicationFactory<Program> _factory = default!;
    protected HttpClient _client = default!;

    // Hardcodes the connection string for a local PostgreSQL instance used exclusively for integration tests.
    private const string TestConnectionString = "Host=localhost;Port=5432;Database=meusitepessoal_testdb;Username=postgres;Password=Anah@1418";

    /// <summary>
    /// Authenticates the HTTP client using the default admin credentials seeded in the database.
    /// </summary>
    protected async Task AuthenticateAsync()
    {
        using var scope = _factory.Services.CreateScope();
        var configuration = scope.ServiceProvider.GetRequiredService<IConfiguration>();
        var adminEmail = configuration["AdminSetup:Email"];
        var adminPassword = configuration["AdminSetup:Password"];

        // The method triggers a login request to the API's authentication endpoint.
        var loginResponse = await _client.PostAsJsonAsync("/api/auth/login", new
        {
            Username = adminEmail,
            Password = adminPassword
        });

        var result = await loginResponse.Content.ReadFromJsonAsync<AuthResult>();

        // The authorization header is configured in the HttpClient to ensure all subsequent requests include the Bearer token.
        _client.DefaultRequestHeaders.Authorization = 
            new AuthenticationHeaderValue("Bearer", result?.Token);
    }

    // Internal DTO captures the login response during test execution.
    private record AuthResult(string Token, string Username);
    
    public async Task InitializeAsync()
    {
        _factory = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                // Configures the environment to bypass development-only startup logic (like database seeding).
                builder.UseEnvironment("Testing");
                
                builder.ConfigureAppConfiguration((context, config) =>
                {
                    config.AddInMemoryCollection(new Dictionary<string, string?>
                    {
                        ["AdminSetup:Email"] = "admin@example.com",
                        ["AdminSetup:Password"] = "Admin123!"
                    });
                });

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
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        var configuration = scope.ServiceProvider.GetRequiredService<Microsoft.Extensions.Configuration.IConfiguration>();
        var logger = scope.ServiceProvider.GetRequiredService<Microsoft.Extensions.Logging.ILogger<BaseIntegrationTest>>();
        
        // Deletes and recreates the database from scratch to guarantee a clean state for every test execution.
        await db.Database.EnsureDeletedAsync();
        await db.Database.MigrateAsync();

        // Seed initial data for testing, including the admin user.
        await DbInitializer.SeedAsync(db, userManager, roleManager, configuration, logger);
        
        // Clean up seeded articles so tests start with an empty article table.
        db.Articles.RemoveRange(db.Articles);
        await db.SaveChangesAsync();
    }

    public async Task DisposeAsync()
    {
        await _factory.DisposeAsync();
    }
}