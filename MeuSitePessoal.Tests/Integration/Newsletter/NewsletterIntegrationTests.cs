using System.Net;
using System.Net.Http.Json;
using MeuSitePessoal.Application.Newsletter.Commands.Subscribe;
using MeuSitePessoal.Domain.Entities;
using MeuSitePessoal.Infrastructure.Data;
using Microsoft.Extensions.DependencyInjection;
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
    public async Task Unsubscribe_ShouldRedirectToFrontend_WhenTokenIsValid()
    {
        // Arrange
        var email = "unsub@test.com";
        string? token = null;
        
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<BlogDbContext>();
        
            // HC: Create using the constructor. 
            var subscriber = new Subscriber(email);
            subscriber.IsVerified = true;
            subscriber.IsActive = true;
        
            // HC: We capture the auto-generated token to use in the request.
            token = subscriber.UnsubscribeToken; 

            db.Subscribers.Add(subscriber);
            await db.SaveChangesAsync();
        }

        // Act
        var response = await _client.GetAsync($"/api/newsletter/unsubscribe?email={email}&token={token}");

        // Assert
        // Since it's a redirect, the status might be 302 or 200 depending on client config,
        // but by default HttpClient follows redirects. 
        // In the controller I used Redirect(url) which is 302.
        Assert.Equal(HttpStatusCode.Found, response.StatusCode);
        Assert.Contains("unsubscribe-success", response.Headers.Location?.ToString() ?? "");

        // Verify in DB
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<BlogDbContext>();
            var sub = db.Subscribers.First(s => s.Email == email);
            Assert.False(sub.IsActive);
        }
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
