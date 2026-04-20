using MeuSitePessoal.Domain.Entities;
using System.Text;
using FluentAssertions;
using MeuSitePessoal.Application.Auth.Commands.ResetPassword;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace MeuSitePessoal.Tests.Unit.Application.Auth.Commands;

/// <summary>
/// Tests the <see cref="ResetPasswordCommandHandler"/> class.
/// </summary>
public class ResetPasswordCommandHandlerTests
{
    private readonly Mock<UserManager<ApplicationUser>> _userManagerMock;
    private readonly Mock<ILogger<ResetPasswordCommandHandler>> _loggerMock;
    private readonly ResetPasswordCommandHandler _sut;

    public ResetPasswordCommandHandlerTests()
    {
        var storeMock = new Mock<IUserStore<ApplicationUser>>();
        _userManagerMock = new Mock<UserManager<ApplicationUser>>(storeMock.Object, null!, null!, null!, null!, null!, null!, null!, null!);
        _loggerMock = new Mock<ILogger<ResetPasswordCommandHandler>>();

        _sut = new ResetPasswordCommandHandler(_userManagerMock.Object, _loggerMock.Object);
    }

    /// <summary>
    /// Verifies that a valid token and user result in a successful password reset.
    /// </summary>
    [Fact]
    public async Task Handle_ValidToken_ReturnsSuccess()
    {
        // Arrange
        var email = "test@test.com";
        var rawToken = "reset-token";
        var encodedToken = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(rawToken));
        var newPassword = "NewSecurePassword123!";
        var command = new ResetPasswordCommand(email, encodedToken, newPassword);
        var user = new ApplicationUser { Email = email };

        _userManagerMock.Setup(x => x.FindByEmailAsync(email)).ReturnsAsync(user);
        _userManagerMock.Setup(x => x.ResetPasswordAsync(user, rawToken, newPassword)).ReturnsAsync(IdentityResult.Success);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.Succeeded.Should().BeTrue();
    }

    /// <summary>
    /// Verifies that the handler returns failure when user is not found.
    /// </summary>
    [Fact]
    public async Task Handle_UserNotFound_ReturnsFailure()
    {
        // Arrange
        var command = new ResetPasswordCommand("notfound@test.com", "token", "pass");
        _userManagerMock.Setup(x => x.FindByEmailAsync(It.IsAny<string>())).ReturnsAsync((ApplicationUser?)null);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.Succeeded.Should().BeFalse();
        result.Errors.Should().Contain("User not found.");
    }

    /// <summary>
    /// Verifies that the handler returns Identity errors when reset fails.
    /// </summary>
    [Fact]
    public async Task Handle_InvalidToken_ReturnsIdentityErrors()
    {
        // Arrange
        var email = "test@test.com";
        var encodedToken = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes("invalid"));
        var command = new ResetPasswordCommand(email, encodedToken, "NewPass123!");
        var user = new ApplicationUser { Email = email };
        var identityError = IdentityResult.Failed(new IdentityError { Description = "Invalid token." });

        _userManagerMock.Setup(x => x.FindByEmailAsync(email)).ReturnsAsync(user);
        _userManagerMock.Setup(x => x.ResetPasswordAsync(user, It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(identityError);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.Succeeded.Should().BeFalse();
        result.Errors.Should().Contain("Invalid token.");
    }
}

