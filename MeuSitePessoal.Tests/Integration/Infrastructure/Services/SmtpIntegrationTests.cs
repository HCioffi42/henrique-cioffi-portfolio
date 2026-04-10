using FluentAssertions;
using MeuSitePessoal.Infrastructure.Configuration;
using MeuSitePessoal.Infrastructure.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Xunit;

namespace MeuSitePessoal.Tests.Integration.Infrastructure.Services;

/// <summary>
/// Performs integration tests for the <see cref="MailKitEmailService"/> using a real SMTP server.
/// Validates the full connection and delivery lifecycle against Mailtrap.
/// </summary>
public class SmtpIntegrationTests
{
    private readonly IOptions<EmailSettings> _options;

    /// <summary>
    /// Initializes the test class by loading settings from appsettings.Test.json.
    /// </summary>
    public SmtpIntegrationTests()
    {
        var configuration = new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("appsettings.Test.json")
            .Build();


        var settings = configuration.GetSection("EmailSettings").Get<EmailSettings>() 
                      ?? throw new InvalidOperationException("EmailSettings section is missing in appsettings.Test.json");

        if (string.IsNullOrEmpty(settings.Host))
        {
            throw new InvalidOperationException("SMTP Host is empty. Check if appsettings.Test.json is loaded correctly.");
        }

        _options = Options.Create(settings);


    }

    /// <summary>
    /// Verifies that an email can be successfully sent to the configured SMTP server.
    /// This test performs real Network I/O and requires valid credentials in appsettings.Test.json.
    /// </summary>
    [Fact]
    public async Task SendEmailAsync_ShouldSuccessfullyDeliverToMailtrap()
    {
        // Arrange
        var service = new MailKitEmailService(_options);
        var recipient = "test-recipient@example.com";
        var subject = "Integration Test: SMTP Delivery";
        var body = "<h1>Success!</h1><p>The SMTP handshake and delivery were successful.</p>";
        var cts = new CancellationTokenSource(TimeSpan.FromSeconds(30)); // 30s timeout for network operation

        // Act
        // We capture the task to check for exceptions, as requested by the Assert requirement.
        var exception = await Record.ExceptionAsync(() => 
            service.SendEmailAsync(recipient, subject, body, cts.Token));

        // Assert
        // Verify that no exception was thrown during Connect, Authenticate, Send, and Disconnect.
        Assert.Null(exception);
    }

    /// <summary>
    /// Verifies that providing incorrect credentials results in a failure related to authentication.
    /// Handles cases where the server might disconnect abruptly (SmtpProtocolException).
    /// </summary>
    [Fact]
    public async Task SendEmailAsync_WithInvalidCredentials_ShouldFailWithAuthenticationError()
    {
        // Arrange: Create a copy of the settings with a guaranteed wrong password.
        var invalidSettings = new EmailSettings
        {
            Host = _options.Value.Host,
            Port = _options.Value.Port,
            Username = _options.Value.Username,
            Password = "wrong-password-" + Guid.NewGuid(),
            SenderEmail = _options.Value.SenderEmail,
            SenderName = _options.Value.SenderName,
            EnableSsl = _options.Value.EnableSsl
        };
        var service = new MailKitEmailService(Options.Create(invalidSettings));
        var cts = new CancellationTokenSource(TimeSpan.FromSeconds(30));

        // Act
        var exception = await Record.ExceptionAsync(() => 
            service.SendEmailAsync("test@test.com", "Fail Test", "Body", cts.Token));

        // Assert
        // Checks if the exception is either an AuthenticationException or a ProtocolException 
        // that contains the "Invalid credentials" message.
        exception.Should().NotBeNull();
    
        bool isAuthException = exception is MailKit.Security.AuthenticationException;
        bool isProtocolAuthFailure = exception is MailKit.Net.Smtp.SmtpProtocolException && 
                                     exception.Message.Contains("Invalid credentials");

        (isAuthException || isProtocolAuthFailure).Should().BeTrue(
            $"Expected an authentication failure, but got {exception.GetType().Name}: {exception.Message}");
    }
}

