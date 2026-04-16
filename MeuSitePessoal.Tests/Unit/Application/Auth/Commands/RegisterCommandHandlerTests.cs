using System.Text;
using FluentAssertions;
using MeuSitePessoal.Application.Auth.Commands.Register;
using MeuSitePessoal.Application.Common.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MeuSitePessoal.Domain.Entities;
using MeuSitePessoal.Domain.Interfaces;
using Moq;
using Xunit;

namespace MeuSitePessoal.Tests.Unit.Application.Auth.Commands;

/// <summary>
/// Tests the <see cref="RegisterCommandHandler"/> class using Moq.
/// Ensures user creation, role assignment, and email delivery are performed correctly.
/// </summary>
public class RegisterCommandHandlerTests
{
    private readonly Mock<UserManager<IdentityUser>> _userManagerMock;
    private readonly Mock<IEmailSender> _emailSenderMock;
    private readonly Mock<IEmailTemplateService> _templateServiceMock;
    private readonly Mock<IConfiguration> _configurationMock;
    private readonly Mock<ILogger<RegisterCommandHandler>> _loggerMock;
    private readonly Mock<ISubscriberRepository> _subscriberRepositoryMock;
    private readonly RegisterCommandHandler _sut;

    public RegisterCommandHandlerTests()
    {
        var storeMock = new Mock<IUserStore<IdentityUser>>();
        _userManagerMock = new Mock<UserManager<IdentityUser>>(storeMock.Object, null!, null!, null!, null!, null!, null!, null!, null!);
        _emailSenderMock = new Mock<IEmailSender>();
        _templateServiceMock = new Mock<IEmailTemplateService>();
        _configurationMock = new Mock<IConfiguration>();
        _loggerMock = new Mock<ILogger<RegisterCommandHandler>>();
        _subscriberRepositoryMock = new Mock<ISubscriberRepository>();

        _sut = new RegisterCommandHandler(
            _userManagerMock.Object,
            _emailSenderMock.Object,
            _templateServiceMock.Object,
            _configurationMock.Object,
            _loggerMock.Object,
            _subscriberRepositoryMock.Object);
    }


    /// <summary>
    /// Verifies that a new user is created, assigned the 'Reader' role, and receives a confirmation email.
    /// </summary>
    [Fact]
    public async Task Handle_SuccessfulRegistration_SendsConfirmationEmail()
    {
        // Arrange
        var command = new RegisterCommand("NewUser", "new@test.com", "SecurePass123!");
        
        _userManagerMock.Setup(x => x.CreateAsync(It.IsAny<IdentityUser>(), command.Password))
            .ReturnsAsync(IdentityResult.Success);
        
        _userManagerMock.Setup(x => x.AddToRoleAsync(It.IsAny<IdentityUser>(), "Reader"))
            .ReturnsAsync(IdentityResult.Success);

        _userManagerMock.Setup(x => x.GenerateEmailConfirmationTokenAsync(It.IsAny<IdentityUser>()))
            .ReturnsAsync("valid-token");
        
        _templateServiceMock.Setup(x => x.RenderTemplateAsync(It.IsAny<string>(), It.IsAny<object>()))
            .ReturnsAsync("<html><body>confirm-email</body></html>");

        _configurationMock.Setup(x => x["ClientSettings:BaseUrl"]).Returns("https://test.com");

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.Succeeded.Should().BeTrue();
        
        // Verify newsletter subscriber was added
        _subscriberRepositoryMock.Verify(x => x.AddAsync(It.Is<Subscriber>(s => s.Email == command.Email)), Times.Once);
        
        // Verify email was sent
        _emailSenderMock.Verify(x => x.SendEmailAsync(
            command.Email,
            It.Is<string>(s => s.Contains("Verify")),
            It.Is<string>(b => b.Contains("confirm-email")),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    /// <summary>
    /// Verifies that the handler returns failure when Identity user creation fails.
    /// </summary>
    [Fact]
    public async Task Handle_UserCreationFails_ReturnsFailureResult()
    {
        // Arrange
        var command = new RegisterCommand("ExistingUser", "test@test.com", "Pass123!");
        var identityError = IdentityResult.Failed(new IdentityError { Description = "Error message" });

        _userManagerMock.Setup(x => x.CreateAsync(It.IsAny<IdentityUser>(), command.Password))
            .ReturnsAsync(identityError);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.Succeeded.Should().BeFalse();
        result.Errors.Should().Contain("Error message");
        
        // Verify email was NOT sent
        _emailSenderMock.Verify(x => x.SendEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}