using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using MeuSitePessoal.Api.Controllers;
using MeuSitePessoal.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace MeuSitePessoal.Tests.Integration.Auth;

/// <summary>
/// Performs end-to-end integration tests for authentication endpoints.
/// </summary>
public class AuthIntegrationTests : BaseIntegrationTest
{
    /// <summary>
    /// Validates that the API returns a 401 Unauthorized status when invalid credentials are provided.
    /// </summary>
    [Fact]
    public async Task Login_WithInvalidCredentials_ShouldReturn401Unauthorized()
    {
        // Arrange
        var invalidRequest = new { Username = "admin@example.com", Password = "WrongPassword123" };

        // Act
        var response = await _client.PostAsJsonAsync("/api/Auth/login", invalidRequest);

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    /// <summary>
    /// Validates that the API returns a 200 OK status and a valid JWT when correct credentials are provided.
    /// </summary>
    [Fact]
    public async Task Login_WithValidCredentials_ShouldReturn200OkWithToken()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var config = scope.ServiceProvider.GetRequiredService<IConfiguration>();
    
        var validRequest = new 
        { 
            Username = config["AdminSetup:Email"], 
            Password = config["AdminSetup:Password"] 
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/Auth/login", validRequest);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    
        // Uses the record to avoid dynamic binding errors.
        var result = await response.Content.ReadFromJsonAsync<AuthTokenResponse>();
        Assert.NotNull(result?.Token);
    }
    
    /// <summary>
    /// The test validates the complete cycle: registering a user and then logging in with their credentials.
    /// It ensures the display name (UserName) is correctly recovered in the login response.
    /// </summary>
    [Fact]
    public async Task RegistrationToLogin_FullCycle_ShouldSucceed()
    {
        // 1. Arrange: Data for a new user.
        var uniqueName = $"User_{Guid.NewGuid().ToString().Substring(0, 8)}";
        var regRequest = new AuthController.RegisterRequest(uniqueName, $"{uniqueName}@test.com", "Password123!");

        // 2. Act: Executes the registration.
        var regResponse = await _client.PostAsJsonAsync("/api/auth/register", regRequest);
        if (!regResponse.IsSuccessStatusCode)
        {
            // HC: Captures the actual exception details to avoid guessing the cause of the 500 error.
            var errorBody = await regResponse.Content.ReadAsStringAsync();
            throw new Exception($"Integration Test Failed at Register. Body: {errorBody}");
        }
        regResponse.EnsureSuccessStatusCode();
        
        await ConfirmUserEmailAsync(uniqueName);

        // 3. Act: Executes the login using the email.
        var loginRequest = new AuthController.LoginRequest($"{uniqueName}@test.com", "Password123!");
        var loginResponse = await _client.PostAsJsonAsync("/api/auth/login", loginRequest);
        loginResponse.EnsureSuccessStatusCode();

        // 4. Assert: Verifies the display name in the final response.
        var result = await loginResponse.Content.ReadFromJsonAsync<LoginResponseDto>();
        
        result?.Username.Should().Be(uniqueName);;

        // 5. Assert: Verify newsletter subscriber was created
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<BlogDbContext>();
            var subscriber = await db.Subscribers.FirstOrDefaultAsync(s => s.Email == $"{uniqueName}@test.com");
            subscriber.Should().NotBeNull();
            subscriber!.IsActive.Should().BeTrue();
            subscriber!.IsVerified.Should().BeTrue(); // Confirmed via ConfirmUserEmailAsync
        }
    }

    // Local helper record for type-safe assertion
    private record LoginResponseDto(string Token, string Username, bool RequiresTwoFactor);
    
    // Internal record ensures type safety for the authentication response.
    private record AuthTokenResponse(string Token);
}