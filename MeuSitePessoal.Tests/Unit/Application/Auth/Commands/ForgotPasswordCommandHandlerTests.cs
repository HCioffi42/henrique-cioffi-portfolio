using System.Text;
using FluentAssertions;
using MeuSitePessoal.Application.Auth.Commands.ForgotPassword;
using MeuSitePessoal.Application.Common.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace MeuSitePessoal.Tests.Unit.Application.Auth.Commands;

/// <summary>
/// Tests the <see cref="ForgotPasswordCommandHandler"/> class.
/// Ensures enumeration protection and correct token generation.
/// </summary>
public class ForgotPasswordCommandHandlerTests
{
    private readonly Mock<UserManager<IdentityUser>> _userManagerMock;
    private readonly Mock<IEmailSender> _emailSenderMock;
    private readonly Mock<IConfiguration> _configurationMock;
    private readonly Mock<ILogger<ForgotPasswordCommandHandler>> _loggerMock;
    private readonly ForgotPasswordCommandHandler _sut;

    public ForgotPasswordCommandHandlerTests()
    {
        var storeMock = new Mock<IUserStore<IdentityUser>>();
        _userManagerMock = new Mock<UserManager<IdentityUser>>(storeMock.Object, null!, null!, null!, null!, null!, null!, null!, null!);
        _emailSenderMock = new Mock<IEmailSender>();
        _configurationMock = new Mock<IConfiguration>();
        _loggerMock = new Mock<ILogger<ForgotPasswordCommandHandler>>();

        _sut = new ForgotPasswordCommandHandler(
            _userManagerMock.Object,
            _emailSenderMock.Object,
            _configurationMock.Object,
            _loggerMock.Object);
    }

    /// <summary>
    /// Verifies that the handler returns success even if the user does not exist (Enumeration Protection).
    /// </summary>
    [Fact]
    public async Task Handle_UserNotFound_ReturnsSuccessWithoutSendingEmail()
    {
        // Arrange
        var command = new ForgotPasswordCommand("nonexistent@test.com");
        _userManagerMock.Setup(x => x.FindByEmailAsync(command.Email)).ReturnsAsync((IdentityUser?)null);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.Succeeded.Should().BeTrue();
        result.Message.Should().Contain("instructions shortly");

        _emailSenderMock.Verify(x => x.SendEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    /// <summary>
    /// Verifies that the handler returns success but does not send an email if the account is not confirmed.
    /// </summary>
    [Fact]
    public async Task Handle_EmailNotConfirmed_ReturnsSuccessWithoutSendingEmail()
    {
        // Arrange
        var email = "unconfirmed@test.com";
        var command = new ForgotPasswordCommand(email);
        var user = new IdentityUser { Email = email };

        _userManagerMock.Setup(x => x.FindByEmailAsync(email)).ReturnsAsync(user);
        _userManagerMock.Setup(x => x.IsEmailConfirmedAsync(user)).ReturnsAsync(false);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.Succeeded.Should().BeTrue();
        _emailSenderMock.Verify(x => x.SendEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    /// <summary>
    /// Verifies that the handler sends a reset email when the user exists and is confirmed.
    /// </summary>
    [Fact]
    public async Task Handle_UserConfirmed_SendsResetEmail()
    {
        // Arrange
        var email = "confirmed@test.com";
        var command = new ForgotPasswordCommand(email);
        var user = new IdentityUser { Email = email, UserName = "User" };

        _userManagerMock.Setup(x => x.FindByEmailAsync(email)).ReturnsAsync(user);
        _userManagerMock.Setup(x => x.IsEmailConfirmedAsync(user)).ReturnsAsync(true);
        _userManagerMock.Setup(x => x.GeneratePasswordResetTokenAsync(user)).ReturnsAsync("reset-token");
        _configurationMock.Setup(x => x["ClientSettings:BaseUrl"]).Returns("https://test.com");

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.Succeeded.Should().BeTrue();
        _emailSenderMock.Verify(x => x.SendEmailAsync(
            email,
            It.Is<string>(s => s.Contains("Reset")),
            It.IsAny<string>(),
            It.IsAny<CancellationToken>()), Times.Once);
    }
}
