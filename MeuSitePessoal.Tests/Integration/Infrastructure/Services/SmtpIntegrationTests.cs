using FluentAssertions;
using MeuSitePessoal.Infrastructure.Services;
using Microsoft.Extensions.Options;
using Xunit;

namespace MeuSitePessoal.Tests.Integration.Infrastructure.Services;

/// <summary>
/// Exercises <see cref="MailKitEmailService"/> against a real SMTP conversation: connect,
/// authenticate, send, disconnect. The server runs in this process on a loopback port, so the
/// coverage no longer depends on a third-party provider being reachable (audit T5).
/// </summary>
public class SmtpIntegrationTests
{
    /// <summary>
    /// Verifies that a message reaches the server with the configured sender and the supplied
    /// recipient, subject and body intact.
    /// </summary>
    [Fact]
    public async Task SendEmailAsync_DeliversTheMessageToTheServer()
    {
        await using var server = await SmtpTestServer.StartAsync();

        var settings = server.BuildSettings();
        var service = new MailKitEmailService(Options.Create(settings));

        await service.SendEmailAsync(
            "recipient@test.local",
            "Integration Test: SMTP Delivery",
            "<h1>Success!</h1><p>The SMTP handshake and delivery were successful.</p>",
            CancellationToken.None);

        var delivered = server.ReceivedMessages.Should().ContainSingle().Subject;

        delivered.To.Mailboxes.Should().ContainSingle()
            .Which.Address.Should().Be("recipient@test.local");
        delivered.From.Mailboxes.Should().ContainSingle()
            .Which.Address.Should().Be(settings.SenderEmail);
        delivered.Subject.Should().Be("Integration Test: SMTP Delivery");
        delivered.HtmlBody.Should().Contain("The SMTP handshake and delivery were successful.");
    }

    /// <summary>
    /// Verifies that rejected credentials surface as an authentication failure rather than being
    /// swallowed, and that nothing is delivered.
    /// </summary>
    [Fact]
    public async Task SendEmailAsync_WithInvalidCredentials_ShouldFailWithAuthenticationError()
    {
        await using var server = await SmtpTestServer.StartAsync();

        var settings = server.BuildSettings(password: "wrong-password-" + Guid.NewGuid());
        var service = new MailKitEmailService(Options.Create(settings));

        var exception = await Record.ExceptionAsync(() => service.SendEmailAsync(
            "recipient@test.local",
            "Fail Test",
            "Body",
            CancellationToken.None));

        exception.Should().BeOfType<MailKit.Security.AuthenticationException>();
        server.ReceivedMessages.Should().BeEmpty();
    }
}
