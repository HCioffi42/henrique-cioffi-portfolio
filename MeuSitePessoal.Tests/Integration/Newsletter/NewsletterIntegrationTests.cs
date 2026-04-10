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
    public async Task Subscribe_ShouldReturn200_WhenEmailIsAlreadySubscribed()
    {
        // Arrange
        var command = new SubscribeToNewsletterCommand("existing@integration.com");
    
        // Act 1: Initial subscription
        var firstResponse = await _client.PostAsJsonAsync("/api/newsletter/subscribe", command);
        firstResponse.EnsureSuccessStatusCode();

        // Act 2: Duplicate subscription (Now returns OK to resend link or confirm status)
        var secondResponse = await _client.PostAsJsonAsync("/api/newsletter/subscribe", command);

        // Assert
        Assert.Equal(HttpStatusCode.OK, secondResponse.StatusCode);
    
        var content = await secondResponse.Content.ReadFromJsonAsync<dynamic>();
        var message = content?.GetProperty("message").GetString() ?? string.Empty;
    
        // Validamos se a mensagem de sucesso padrão foi retornada
        Assert.Contains("Subscription initiated", message);
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
}
