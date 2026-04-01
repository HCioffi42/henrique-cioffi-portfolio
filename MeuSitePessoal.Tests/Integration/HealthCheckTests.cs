using System.Net;
using Xunit;

namespace MeuSitePessoal.Tests.Integration;

/**
 * Verifies that the health check endpoint is correctly exposed and operational.
 */
public class HealthCheckTests : BaseIntegrationTest
{
    /**
     * Checks if the /health endpoint returns a 200 OK status, confirming service vitality.
     */
    [Fact]
    public async Task HealthCheck_ReturnsOk()
    {
        // Act: Request the health endpoint anonymously.
        var response = await _client.GetAsync("/health");

        // Assert: Ensure the response indicates a healthy state.
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        
        var content = await response.Content.ReadAsStringAsync();
        Assert.Contains("Healthy", content);
    }
}
