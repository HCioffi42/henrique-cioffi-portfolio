using System.Net.Http.Json;
using FluentAssertions;
using MeuSitePessoal.Domain.Entities;
using MeuSitePessoal.Infrastructure.Configuration;
using MeuSitePessoal.Infrastructure.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Xunit;

namespace MeuSitePessoal.Tests.Integration.Auth;

/// <summary>
/// Covers Identity lockout and the administrative exemption.
/// Lockout state is persisted on the user row, so these run against the real database rather
/// than asserting on a substituted sign-in manager.
/// </summary>
public class AccountLockoutTests : BaseIntegrationTest
{
    private const string ReaderName = "lockout_reader";
    private const string ReaderEmail = "lockout_reader@test.com";
    private const string ReaderPassword = "Password123!";
    private const string AdminEmail = "admin@example.com";
    private const string AdminPassword = "Admin123!";
    private const string SecondAdminName = "second_admin";
    private const string SecondAdminEmail = "second_admin@test.com";

    /// <summary>The threshold declared by the lockout policy.</summary>
    private const int MaxFailedAccessAttempts = 5;

    private async Task RegisterConfirmedReaderAsync()
    {
        await _client.PostAsJsonAsync("/api/auth/register", new
        {
            UserName = ReaderName,
            Email = ReaderEmail,
            Password = ReaderPassword
        });

        await ConfirmUserEmailAsync(ReaderName);
    }

    private Task<HttpResponseMessage> SignInAsync(string username, string password)
        => _client.PostAsJsonAsync("/api/auth/login", new { Username = username, Password = password });

    /// <summary>
    /// Reads a user row through a scope of its own, so the assertion observes persisted state
    /// rather than an instance still tracked by an earlier operation.
    /// </summary>
    private async Task<ApplicationUser> GetUserAsync(string nameOrEmail)
    {
        using var scope = _factory.Services.CreateScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

        var user = await userManager.FindByNameAsync(nameOrEmail)
                   ?? await userManager.FindByEmailAsync(nameOrEmail);

        user.Should().NotBeNull($"the test requires '{nameOrEmail}' to exist");
        return user!;
    }

    /// <summary>Runs the seeder again against the current database, as a restart would.</summary>
    private async Task RunSeedAsync()
    {
        using var scope = _factory.Services.CreateScope();

        await DbInitializer.SeedAsync(
            scope.ServiceProvider.GetRequiredService<BlogDbContext>(),
            scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>(),
            scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>(),
            scope.ServiceProvider.GetRequiredService<IConfiguration>(),
            scope.ServiceProvider.GetRequiredService<ILogger<AccountLockoutTests>>());
    }

    /// <summary>Applies lockout state directly, bypassing the UserManager guards that refuse it on an exempt account.</summary>
    private async Task MutateUserAsync(string nameOrEmail, Action<ApplicationUser> mutate)
    {
        using var scope = _factory.Services.CreateScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

        var user = await userManager.FindByNameAsync(nameOrEmail)
                   ?? await userManager.FindByEmailAsync(nameOrEmail);

        mutate(user!);
        await userManager.UpdateAsync(user!);
    }

    [Fact]
    public async Task FailedSignIn_IncrementsLockoutCounter_ForLockoutEnabledAccount()
    {
        await RegisterConfirmedReaderAsync();

        await SignInAsync(ReaderName, "wrong_password");

        var user = await GetUserAsync(ReaderName);
        user.AccessFailedCount.Should().Be(1);
    }

    [Fact]
    public async Task Account_LocksOut_AfterConfiguredFailedAttempts()
    {
        await RegisterConfirmedReaderAsync();

        for (var i = 0; i < MaxFailedAccessAttempts; i++)
        {
            await SignInAsync(ReaderName, "wrong_password");
        }

        var user = await GetUserAsync(ReaderName);
        user.LockoutEnd.Should().NotBeNull();
        user.LockoutEnd!.Value.Should().BeAfter(DateTimeOffset.UtcNow);
    }

    [Fact]
    public async Task SeededAdministrator_HasLockoutDisabled()
    {
        var admin = await GetUserAsync(AdminEmail);
        admin.LockoutEnabled.Should().BeFalse();
    }

    [Fact]
    public async Task Administrator_DoesNotLockOut_AfterRepeatedFailedAttempts()
    {
        for (var i = 0; i < MaxFailedAccessAttempts * 2; i++)
        {
            await SignInAsync(AdminEmail, "wrong_password");
        }

        var response = await SignInAsync(AdminEmail, AdminPassword);
        var result = await response.Content.ReadFromJsonAsync<LoginResponse>();

        result!.Token.Should().NotBeNullOrEmpty("the administrative account must remain reachable");
    }

    [Fact]
    public async Task ExistingAdministrator_AlreadyInRole_HasLockoutDisabledOnNextSeed()
    {
        // Constructs the production shape the suite never otherwise produces: an administrator
        // already holding the role, so the promotion branch does not run on the next startup.
        await MutateUserAsync(AdminEmail, user => user.LockoutEnabled = true);
        (await GetUserAsync(AdminEmail)).LockoutEnabled.Should().BeTrue("the arrangement must take effect");

        await RunSeedAsync();

        var admin = await GetUserAsync(AdminEmail);
        admin.LockoutEnabled.Should().BeFalse();
    }

    [Fact]
    public async Task ExemptAdministrator_HasLockoutCountersCleared()
    {
        // AccessFailedAsync writes these without consulting LockoutEnabled, so an exempt account
        // accumulates state that would lock it the moment the exemption were lifted.
        await MutateUserAsync(AdminEmail, user =>
        {
            user.AccessFailedCount = 3;
            user.LockoutEnd = DateTimeOffset.UtcNow.AddMinutes(30);
        });

        await RunSeedAsync();

        var admin = await GetUserAsync(AdminEmail);
        admin.AccessFailedCount.Should().Be(0);
        admin.LockoutEnd.Should().BeNull();
    }

    [Fact]
    public async Task LockedOutResponse_IsIndistinguishableFromWrongPassword()
    {
        await RegisterConfirmedReaderAsync();

        var wrongPasswordResponse = await SignInAsync(ReaderName, "wrong_password");
        var wrongPasswordBody = await wrongPasswordResponse.Content.ReadAsStringAsync();

        for (var i = 0; i < MaxFailedAccessAttempts; i++)
        {
            await SignInAsync(ReaderName, "wrong_password");
        }

        // The correct password against a locked account is the case that would leak the state,
        // because only lockout can explain the refusal.
        var lockedOutResponse = await SignInAsync(ReaderName, ReaderPassword);
        var lockedOutBody = await lockedOutResponse.Content.ReadAsStringAsync();

        lockedOutResponse.StatusCode.Should().Be(wrongPasswordResponse.StatusCode);
        lockedOutBody.Should().Be(wrongPasswordBody);
    }


    /// <summary>
    /// Runs the seeder against a configuration that declares no AdminSetup section, which is the
    /// shape a deployed environment presents when the seed credentials are not supplied.
    /// </summary>
    private async Task RunSeedWithoutAdminSetupAsync()
    {
        using var scope = _factory.Services.CreateScope();

        var emptyConfiguration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>())
            .Build();

        await DbInitializer.SeedAsync(
            scope.ServiceProvider.GetRequiredService<BlogDbContext>(),
            scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>(),
            scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>(),
            emptyConfiguration,
            scope.ServiceProvider.GetRequiredService<ILogger<AccountLockoutTests>>());
    }

    [Fact]
    public async Task AdministratorExemption_Converges_WhenAdminSetupIsNotConfigured()
    {
        // The deployed environment supplies no AdminSetup credentials, so the seeder's admin
        // block is skipped in full. The exemption must not depend on that block running.
        await MutateUserAsync(AdminEmail, user => user.LockoutEnabled = true);

        await RunSeedWithoutAdminSetupAsync();

        var admin = await GetUserAsync(AdminEmail);
        admin.LockoutEnabled.Should().BeFalse();
    }

    [Fact]
    public async Task EveryAdministrator_IsExempt_NotOnlyTheConfiguredAccount()
    {
        // The exemption is a property of the role, not of the account named in configuration.
        // A deployment may hold several administrators; the seeder knows of at most one.
        using (var scope = _factory.Services.CreateScope())
        {
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

            var secondAdmin = new ApplicationUser
            {
                UserName = SecondAdminName,
                Email = SecondAdminEmail,
                EmailConfirmed = true,
                PreferredLanguage = "en"
            };

            (await userManager.CreateAsync(secondAdmin, "Password123!")).Succeeded
                .Should().BeTrue("the arrangement requires a second administrator");
            await userManager.AddToRoleAsync(secondAdmin, "Admin");
        }

        await MutateUserAsync(SecondAdminName, user =>
        {
            user.LockoutEnabled = true;
            user.AccessFailedCount = 4;
        });

        await RunSeedAsync();

        var secondAdministrator = await GetUserAsync(SecondAdminName);
        secondAdministrator.LockoutEnabled.Should().BeFalse();
        secondAdministrator.AccessFailedCount.Should().Be(0);
    }
    private sealed record LoginResponse(string? Token, string? Username);
}
