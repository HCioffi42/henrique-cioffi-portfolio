using System.Net;
using System.Net.Http.Json;
using MeuSitePessoal.Application.Newsletter.Commands.Subscribe;
using Xunit;

namespace MeuSitePessoal.Tests.Integration.Newsletter;

public class NewsletterIntegrationTests : BaseIntegrationTest
{
    [Fact]
    public async Task Subscribe_ShouldReturn200_WhenEmailIsNew()
    {
        // Arrange
        var command = new SubscribeToNewsletterCommand("test@integration.com");

        // Act
        var response = await _client.PostAsJsonAsync("/api/newsletter/subscribe", command);

        // Assert
        response.EnsureSuccessStatusCode();
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Subscribe_ShouldReturn400_WhenEmailIsInvalid()
    {
        // Arrange
        var command = new SubscribeToNewsletterCommand("not-an-email");

        // Act
        var response = await _client.PostAsJsonAsync("/api/newsletter/subscribe", command);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Subscribe_ShouldReturn409_WhenEmailIsAlreadySubscribed()
    {
        // Arrange
        var command = new SubscribeToNewsletterCommand("existing@integration.com");
        
        // Act 1: Initial subscription
        var firstResponse = await _client.PostAsJsonAsync("/api/newsletter/subscribe", command);
        firstResponse.EnsureSuccessStatusCode();

        // Act 2: Duplicate subscription
        var secondResponse = await _client.PostAsJsonAsync("/api/newsletter/subscribe", command);

        // Assert
        Assert.Equal(HttpStatusCode.Conflict, secondResponse.StatusCode);
        
        var content = await secondResponse.Content.ReadFromJsonAsync<dynamic>();
        Assert.Contains("already subscribed", content?.GetProperty("message").GetString() ?? string.Empty);
    }
}
