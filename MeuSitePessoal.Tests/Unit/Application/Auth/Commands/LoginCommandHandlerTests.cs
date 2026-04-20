using MeuSitePessoal.Domain.Entities;
using FluentAssertions;
using MeuSitePessoal.Application.Auth.Commands.Login;
using MeuSitePessoal.Application.Common.Interfaces;
using MeuSitePessoal.Domain.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace MeuSitePessoal.Tests.Unit.Application.Auth.Commands;

/// <summary>
/// Tests the <see cref="LoginCommandHandler"/> logic.
/// Validates hybrid login (email/username) and feature-toggled 2FA behavior.
/// </summary>
public class LoginCommandHandlerTests
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly ITokenService _tokenService;
    private readonly IFeatureToggleService _featureToggle;
    private readonly LoginCommandHandler _sut;

    public LoginCommandHandlerTests()
    {
        // Mocks complex Identity dependencies using NSubstitute.
        var store = Substitute.For<IUserStore<ApplicationUser>>();
        _userManager = Substitute.For<UserManager<ApplicationUser>>(store, null, null, null, null, null, null, null, null);
        
        var contextAccessor = Substitute.For<IHttpContextAccessor>();
        contextAccessor.HttpContext.Returns(new DefaultHttpContext());
        
        var claimsFactory = Substitute.For<IUserClaimsPrincipalFactory<ApplicationUser>>();
        
        _signInManager = Substitute.For<SignInManager<ApplicationUser>>(_userManager, contextAccessor, claimsFactory, null, null, null, null);
        
        _tokenService = Substitute.For<ITokenService>();
        _featureToggle = Substitute.For<IFeatureToggleService>();
        
        _sut = new LoginCommandHandler(
            _userManager,
            _signInManager,
            _tokenService,
            _featureToggle,
            Substitute.For<ILogger<LoginCommandHandler>>());
    }

    /// <summary>
    /// The test verifies that login succeeds using either the username or the email address.
    /// It covers the hybrid search logic implemented in the handler.
    /// </summary>
    [Theory]
    [InlineData("Cioffi")] // Test by username
    [InlineData("henrique@test.com")] // Test by email
    public async Task Handle_WithValidCredentials_ReturnsSuccessfulResult(string identifier)
    {
        // Arrange
        var user = new ApplicationUser { UserName = "Cioffi", Email = "henrique@test.com" };
    
        // This prevents the "await null" exception.
        _userManager.FindByNameAsync(Arg.Any<string>()).Returns(Task.FromResult<ApplicationUser?>(null));
        _userManager.FindByEmailAsync(Arg.Any<string>()).Returns(Task.FromResult<ApplicationUser?>(null));
        
        // Now setup the specific successful matches.
        _userManager.FindByNameAsync("Cioffi").Returns(user);
        _userManager.FindByEmailAsync("henrique@test.com").Returns(user);
    
        // HC: Must mock GetRolesAsync because the handler calls it before generating the token.
        _userManager.GetRolesAsync(Arg.Any<ApplicationUser>()).Returns(new List<string> { "Reader" });
    
        _signInManager.CheckPasswordSignInAsync(Arg.Any<ApplicationUser>(), Arg.Any<string>(), false)
            .Returns(SignInResult.Success);
    
        _featureToggle.Is2FAEnabled().Returns(false);
        _tokenService.GenerateToken(Arg.Any<ApplicationUser>(), Arg.Any<IList<string>>()).Returns("valid-jwt-token");

        // Act
        var command = new LoginCommand(identifier, "Password123!");
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.Token.Should().Be("valid-jwt-token");
        result.Username.Should().Be("Cioffi");
    }
    
    /// <summary>
    /// The test verifies that the handler returns a failure result when the user is not found by either identifier.
    /// </summary>
    [Fact]
    public async Task Handle_WhenUserDoesNotExist_ReturnsFailure()
    {
        // Arrange: Mock returns null for both name and email search.
        _userManager.FindByNameAsync(Arg.Any<string>()).Returns(Task.FromResult<ApplicationUser?>(null));
        _userManager.FindByEmailAsync(Arg.Any<string>()).Returns(Task.FromResult<ApplicationUser?>(null));

        // Act
        var result = await _sut.Handle(new LoginCommand("ghost_user", "any_pass"), CancellationToken.None);

        // Assert
        result.Token.Should().BeNull();
        result.Username.Should().BeNull();
    }

    /// <summary>
    /// The test verifies that the handler returns a failure result when the provided password does not match the stored hash.
    /// </summary>
    [Fact]
    public async Task Handle_WhenPasswordIsWrong_ReturnsFailure()
    {
        // Arrange: User exists but password check fails.
        var user = new ApplicationUser { UserName = "Cioffi" };
        _userManager.FindByNameAsync("Cioffi").Returns(user);
        _signInManager.CheckPasswordSignInAsync(user, "wrong_pass", false)
            .Returns(Microsoft.AspNetCore.Identity.SignInResult.Failed);

        // Act
        var result = await _sut.Handle(new LoginCommand("Cioffi", "wrong_pass"), CancellationToken.None);

        // Assert
        result.Token.Should().BeNull();
    }
    
    /// <summary>
    /// The test verifies that the handler signals a 2FA requirement when the corresponding feature toggle is enabled.
    /// </summary>
    [Fact]
    public async Task Handle_When2FAIsEnabled_ReturnsRequiresTwoFactor()
    {
        // Arrange
        var user = new ApplicationUser { UserName = "Cioffi" };
        _userManager.FindByNameAsync("Cioffi").Returns(user);
        _signInManager.CheckPasswordSignInAsync(user, "pass", false)
            .Returns(Microsoft.AspNetCore.Identity.SignInResult.Success);
    
        // HC: Forcing the feature toggle to return true.
        _featureToggle.Is2FAEnabled().Returns(true);

        // Act
        var result = await _sut.Handle(new LoginCommand("Cioffi", "pass"), CancellationToken.None);

        // Assert: Token must be null and flag must be true.
        result.RequiresTwoFactor.Should().BeTrue();
        result.Token.Should().BeNull();
    }
}
