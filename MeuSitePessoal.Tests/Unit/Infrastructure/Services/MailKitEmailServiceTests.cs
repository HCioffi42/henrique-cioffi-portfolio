using FluentAssertions;
using MeuSitePessoal.Infrastructure.Configuration;
using MeuSitePessoal.Infrastructure.Services;
using Microsoft.Extensions.Options;
using MimeKit;
using Moq;
using Xunit;

namespace MeuSitePessoal.Tests.Unit.Infrastructure.Services;

/// <summary>
/// Contains unit tests for the <see cref="MailKitEmailService"/>.
/// </summary>
public class MailKitEmailServiceTests
{
    private readonly Mock<IOptions<EmailSettings>> _optionsMock;
    private readonly EmailSettings _settings;

    /// <summary>
    /// Initializes a new instance of the testing class with a mocked configuration.
    /// </summary>
    public MailKitEmailServiceTests()
    {
        _optionsMock = new Mock<IOptions<EmailSettings>>();
        _settings = new EmailSettings
        {
            Host = "smtp.test.com",
            Port = 587,
            Username = "user",
            Password = "pass",
            SenderEmail = "sender@test.com",
            SenderName = "Test Sender",
            EnableSsl = true
        };
        _optionsMock.Setup(x => x.Value).Returns(_settings);
    }

    /// <summary>
    /// Verifies that the internal message creation logic correctly maps settings and parameters to a MimeMessage.
    /// </summary>
    [Fact]
    public void CreateMessage_ShouldMapSettingsAndParametersCorrectly()
    {
        // Arrange
        var service = new MailKitEmailService(_optionsMock.Object);
        var recipient = "recipient@test.com";
        var subject = "Test Subject";
        var body = "<p>Test Body</p>";

        // Act
        var message = service.CreateMessage(recipient, subject, body);

        // Assert
        ((IEnumerable<InternetAddress>)message.From).Should().ContainSingle().Which.As<MailboxAddress>().Address.Should().Be(_settings.SenderEmail);
        ((IEnumerable<InternetAddress>)message.From).Should().ContainSingle().Which.As<MailboxAddress>().Name.Should().Be(_settings.SenderName);
        ((IEnumerable<InternetAddress>)message.To).Should().ContainSingle().Which.As<MailboxAddress>().Address.Should().Be(recipient);
        message.Subject.Should().Be(subject);
        message.HtmlBody.Should().Be(body);

    }

    /// <summary>
    /// Verifies that an exception is thrown when an invalid email address is provided.
    /// </summary>
    [Fact]
    public void CreateMessage_ShouldThrowException_WhenRecipientEmailIsInvalid()
    {
        // Arrange
        var service = new MailKitEmailService(_optionsMock.Object);
        var invalidRecipient = string.Empty;


        // Act
        Action act = () => service.CreateMessage(invalidRecipient, "Subject", "Body");

        // Assert
        act.Should().Throw<ParseException>();
    }

    /// <summary>
    /// Verifies that the message construction handles an empty body correctly.
    /// </summary>
    [Fact]
    public void CreateMessage_ShouldHandleEmptyBody()
    {
        // Arrange
        var service = new MailKitEmailService(_optionsMock.Object);
        var emptyBody = string.Empty;

        // Act
        var message = service.CreateMessage("test@test.com", "Subject", emptyBody);

        // Assert
        message.HtmlBody.Should().Be(emptyBody);
    }
}
