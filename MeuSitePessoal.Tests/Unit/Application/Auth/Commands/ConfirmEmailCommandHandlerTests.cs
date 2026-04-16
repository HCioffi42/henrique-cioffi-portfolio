using System.Text;
using FluentAssertions;
using MeuSitePessoal.Application.Auth.Commands.ConfirmEmail;
using MeuSitePessoal.Domain.Entities;
using MeuSitePessoal.Domain.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace MeuSitePessoal.Tests.Unit.Application.Auth.Commands;

/// <summary>
/// Tests the <see cref="ConfirmEmailCommandHandler"/> class.
/// </summary>
public class ConfirmEmailCommandHandlerTests
{
    private readonly Mock<UserManager<IdentityUser>> _userManagerMock;
    private readonly Mock<ILogger<ConfirmEmailCommandHandler>> _loggerMock;
    private readonly Mock<ISubscriberRepository> _subscriberRepositoryMock;
    private readonly ConfirmEmailCommandHandler _sut;

    public ConfirmEmailCommandHandlerTests()
    {
        var storeMock = new Mock<IUserStore<IdentityUser>>();
        _userManagerMock = new Mock<UserManager<IdentityUser>>(storeMock.Object, null!, null!, null!, null!, null!, null!, null!, null!);
        _loggerMock = new Mock<ILogger<ConfirmEmailCommandHandler>>();
        _subscriberRepositoryMock = new Mock<ISubscriberRepository>();

        _sut = new ConfirmEmailCommandHandler(_userManagerMock.Object, _loggerMock.Object, _subscriberRepositoryMock.Object);
    }

    /// <summary>
    /// Verifies that a valid token and user ID result in a successful email confirmation.
    /// </summary>
    [Fact]
    public async Task Handle_ValidToken_ReturnsSuccess()
    {
        // Arrange
        var userId = "user-id";
        var email = "test@test.com";
        var subscriber = new Subscriber(email);
        var rawToken = subscriber.VerificationToken!;
        var encodedToken = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(rawToken));
        var command = new ConfirmEmailCommand(userId, encodedToken);
        var user = new IdentityUser { Id = userId, Email = email };

        _userManagerMock.Setup(x => x.FindByIdAsync(userId)).ReturnsAsync(user);
        _userManagerMock.Setup(x => x.ConfirmEmailAsync(user, rawToken)).ReturnsAsync(IdentityResult.Success);
        _subscriberRepositoryMock.Setup(x => x.GetByEmailAsync(email)).ReturnsAsync(subscriber);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.Succeeded.Should().BeTrue();
        subscriber.IsVerified.Should().BeTrue();
        _subscriberRepositoryMock.Verify(x => x.UpdateAsync(subscriber), Times.Once);
    }

    /// <summary>
    /// Verifies that the handler returns failure when the user is not found.
    /// </summary>
    [Fact]
    public async Task Handle_UserNotFound_ReturnsFailure()
    {
        // Arrange
        var command = new ConfirmEmailCommand("unknown", "token");
        _userManagerMock.Setup(x => x.FindByIdAsync(It.IsAny<string>())).ReturnsAsync((IdentityUser?)null);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.Succeeded.Should().BeFalse();
        result.Errors.Should().Contain("User not found.");
    }

    /// <summary>
    /// Verifies that the handler returns Identity errors when token confirmation fails.
    /// </summary>
    [Fact]
    public async Task Handle_InvalidToken_ReturnsIdentityErrors()
    {
        // Arrange
        var userId = "user-id";
        var encodedToken = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes("invalid"));
        var command = new ConfirmEmailCommand(userId, encodedToken);
        var user = new IdentityUser { Id = userId };
        var identityError = IdentityResult.Failed(new IdentityError { Description = "Invalid token." });

        _userManagerMock.Setup(x => x.FindByIdAsync(userId)).ReturnsAsync(user);
        _userManagerMock.Setup(x => x.ConfirmEmailAsync(user, It.IsAny<string>())).ReturnsAsync(identityError);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.Succeeded.Should().BeFalse();
        result.Errors.Should().Contain("Invalid token.");
    }
}
