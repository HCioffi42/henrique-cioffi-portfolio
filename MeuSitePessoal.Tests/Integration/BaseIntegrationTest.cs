using MeuSitePessoal.Domain.Entities;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using MeuSitePessoal.Api;
using MeuSitePessoal.Api.Configuration;
using MeuSitePessoal.Application.Common.Interfaces;
using MeuSitePessoal.Infrastructure.Configuration;
using MeuSitePessoal.Infrastructure.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;

namespace MeuSitePessoal.Tests.Integration;

public class BaseIntegrationTest : IAsyncLifetime
{
    protected WebApplicationFactory<Program> _factory = default!;
    protected HttpClient _client = default!;

    /// <summary>
    /// Publishes the signing key to the process environment before any host is constructed.
    /// See <see cref="TestHostEnvironment"/> for why the environment is the only source reaching
    /// the build-time read performed during startup.
    /// </summary>
    static BaseIntegrationTest()
    {
        TestHostEnvironment.Apply();
    }

    /// <summary>
    /// Extra configuration a derived fixture contributes, applied over the defaults below.
    /// Tests asserting throttling override the thresholds here rather than duplicating the
    /// database and mail setup in a second fixture.
    /// </summary>
    protected virtual IDictionary<string, string?> AdditionalConfiguration
        => new Dictionary<string, string?>();

    /// <summary>
    /// Thresholds high enough that no ordinary test reaches them. The limiter stays registered
    /// and active in every environment so a wiring regression is caught rather than hidden; only
    /// the numbers differ. The in-memory host resolves no client address, so every request in a
    /// test shares one partition, and a test authenticating several times would otherwise consume
    /// a realistic budget.
    /// </summary>
    private static Dictionary<string, string?> BuildDefaultConfiguration()
    {
        var values = new Dictionary<string, string?>
        {
            ["AdminSetup:Email"] = "admin@example.com",
            ["AdminSetup:Password"] = "Admin123!",
            // HC: Essential for the Newsletter Handler to generate links during integration tests.
            ["ApiSettings:BaseUrl"] = "http://localhost:25683",
            ["ClientSettings:BaseUrl"] = "http://localhost:5173",
            // Mirrors the environment variable applied by TestHostEnvironment, so any consumer
            // resolving the key after the host is built sees the same value.
            ["JwtSettings:Key"] = TestHostEnvironment.SigningKey
        };

        foreach (var policy in RateLimitPolicies.All)
        {
            values[$"RateLimiting:Policies:{policy}:PermitLimit"] = "100000";
            values[$"RateLimiting:Policies:{policy}:WindowMinutes"] = "60";
        }

        return values;
    }

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
    
    /// <summary>
    /// Manually marks a user's email as confirmed in the Identity database.
    /// This allows the test suite to proceed with login operations without a real verification token,
    /// bypassing the RequireConfirmedEmail policy.
    /// </summary>
    /// <param name="username">The name of the user whose email will be confirmed.</param>
    protected async Task ConfirmUserEmailAsync(string username)
    {
        using var scope = _factory.Services.CreateScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var user = await userManager.FindByNameAsync(username);

        if (user == null)
        {
            throw new InvalidOperationException($"Integration Test Failure: User '{username}' was not found.");
        }

        user.EmailConfirmed = true;
        await userManager.UpdateAsync(user);

        // HC: Also verify and ACTIVATE the newsletter subscriber for integration tests.
        var db = scope.ServiceProvider.GetRequiredService<BlogDbContext>();
        var subscriber = await db.Subscribers.FirstOrDefaultAsync(s => s.Email == user.Email);
        
        if (subscriber != null)
        {
            subscriber.IsVerified = true;
            subscriber.IsActive = true; 
            subscriber.VerifiedAt = DateTime.UtcNow;
        
            await db.SaveChangesAsync();
        }
    }
    
    public async Task InitializeAsync()
    {
        _factory = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                // Configures the environment to bypass development-only startup logic (like database seeding).
                builder.UseEnvironment("Testing");
                
                builder.ConfigureAppConfiguration((context, config) =>
                {
                    var values = BuildDefaultConfiguration();

                    // A derived fixture's contributions take precedence over the defaults.
                    foreach (var entry in AdditionalConfiguration)
                    {
                        values[entry.Key] = entry.Value;
                    }

                    config.AddInMemoryCollection(values);
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

                    // Injects the DbContext configured to use the dedicated test database.
                    services.AddDbContext<BlogDbContext>(options =>
                    {
                        options.UseNpgsql(TestDatabase.ConnectionString);
                    });
                    
                    var emailSenderDescriptor = services.SingleOrDefault(
                        d => d.ServiceType == typeof(IEmailSender));

                    if (emailSenderDescriptor != null)
                    {
                        services.Remove(emailSenderDescriptor);
                    }

                    // HC: Re-adding the Mock for IEmailSender to ensure it's properly registered.
                    services.AddSingleton<IEmailSender>(sp => new Mock<IEmailSender>().Object);
                    // services.AddSingleton<IEmailTemplateService>(sp => new Mock<IEmailTemplateService>().Object);
                });
            });

        _client = _factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });
        
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<BlogDbContext>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        var configuration = scope.ServiceProvider.GetRequiredService<Microsoft.Extensions.Configuration.IConfiguration>();
        var logger = scope.ServiceProvider.GetRequiredService<Microsoft.Extensions.Logging.ILogger<BaseIntegrationTest>>();
        
        // Returns the database to an empty state. The schema is built once for the whole run and
        // reused; see TestDatabase for why this is not a drop and re-migrate per test.
        await TestDatabase.ResetAsync(db);

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
