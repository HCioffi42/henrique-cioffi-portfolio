using FluentAssertions;
using MeuSitePessoal.Application.Auth.Commands.Register;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;

namespace MeuSitePessoal.Tests.Unit.Application.Auth.Commands;

/// <summary>
/// Tests the <see cref="RegisterCommandHandler"/> class.
/// Ensures user creation and role assignment are performed correctly.
/// </summary>
public class RegisterCommandHandlerTests
{
    private readonly UserManager<IdentityUser> _userManager;
    private readonly RegisterCommandHandler _sut;

    public RegisterCommandHandlerTests()
    {
        var store = Substitute.For<IUserStore<IdentityUser>>();
        _userManager = Substitute.For<UserManager<IdentityUser>>(store, null, null, null, null, null, null, null, null);
        
        _sut = new RegisterCommandHandler(_userManager, Substitute.For<ILogger<RegisterCommandHandler>>());
    }

    /// <summary>
    /// The test verifies that a new user is created and assigned the 'Reader' role upon success.
    /// </summary>
    [Fact]
    public async Task Handle_WhenRegistrationSucceeds_ReturnsSuccessfulResult()
    {
        // Arrange: Mokes successful user creation and role addition.
        var command = new RegisterCommand("NewUser", "new@test.com", "SecurePass123!");
        _userManager.CreateAsync(Arg.Any<IdentityUser>(), command.Password)
            .Returns(IdentityResult.Success);
        
        _userManager.AddToRoleAsync(Arg.Any<IdentityUser>(), "Reader")
            .Returns(IdentityResult.Success);

        // Act: Executes the registration process.
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert: Validates the success flag.
        result.Succeeded.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }
    
    /// <summary>
    /// The test verifies that the handler returns a failure result with specific error messages 
    /// when the registration fails due to a duplicate username.
    /// </summary>
    [Fact]
    public async Task Handle_WhenUserNameAlreadyExists_ReturnsFailureWithErrors()
    {
        // Arrange: Mocking a failure from Identity (e.g., Duplicate UserName).
        var command = new RegisterCommand("ExistingUser", "new@test.com", "Pass123!");
        var identityError = IdentityResult.Failed(new IdentityError { Description = "Username 'ExistingUser' is already taken." });
    
        _userManager.CreateAsync(Arg.Any<IdentityUser>(), command.Password).Returns(identityError);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.Succeeded.Should().BeFalse();
        result.Errors.Should().Contain("Username 'ExistingUser' is already taken.");
    }
}