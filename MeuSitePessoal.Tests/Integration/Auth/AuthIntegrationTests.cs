using System.Net;
using System.Net.Http.Json;
using Xunit;

namespace MeuSitePessoal.Tests.Integration.Auth;

public class AuthIntegrationTests : BaseIntegrationTest
{
    [Fact]
    public async Task Login_WithInvalidCredentials_ShouldReturn401Unauthorized()
    {
        // Arrange
        var invalidRequest = new { Username = "admin", Password = "WrongPassword123" };

        // Act
        var response = await _client.PostAsJsonAsync("/api/Auth/login", invalidRequest);

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Login_WithValidCredentials_ShouldReturn200OkWithToken()
    {
        // Arrange
        var validRequest = new { Username = "admin", Password = "Admin123!" };

        // Act
        var response = await _client.PostAsJsonAsync("/api/Auth/login", validRequest);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    
        // Uses the record to avoid dynamic binding errors.
        var result = await response.Content.ReadFromJsonAsync<AuthTokenResponse>();
        Assert.NotNull(result?.Token);
    }

    // Internal record ensures type safety for the authentication response.
    private record AuthTokenResponse(string Token);
}