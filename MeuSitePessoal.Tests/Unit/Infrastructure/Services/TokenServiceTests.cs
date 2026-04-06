using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using FluentAssertions;
using MeuSitePessoal.Infrastructure.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Moq;
using Xunit;

namespace MeuSitePessoal.Tests.Unit.Infrastructure.Services;

/// <summary>
/// Unit tests for the TokenService implementation, validating JWT generation and claim assignment.
/// </summary>
public class TokenServiceTests
{
    private readonly Mock<IConfigurationSection> _jwtSectionMock;
    private readonly TokenService _sut; // System Under Test

    public TokenServiceTests()
    {
        var configMock = new Mock<IConfiguration>();
        _jwtSectionMock = new Mock<IConfigurationSection>();
        _sut = new TokenService(configMock.Object);

        // Sets up the GetSection call to return the mocked section instead of null.
        configMock.Setup(c => c.GetSection("JwtSettings")).Returns(_jwtSectionMock.Object);

        // Configures the values within the JwtSettings section.
        _jwtSectionMock.Setup(s => s["Key"]).Returns("super_secret_key_for_testing_purposes_only_32_chars");
        _jwtSectionMock.Setup(s => s["Issuer"]).Returns("TestIssuer");
        _jwtSectionMock.Setup(s => s["Audience"]).Returns("TestAudience");
        _jwtSectionMock.Setup(s => s["ExpiresInMinutes"]).Returns("60");
    }

    /// <summary>
    /// Validates that the generated JWT contains the expected claims, such as Sub, Email, Name, Roles, and Jti.
    /// </summary>
    [Fact]
    public void GenerateToken_ShouldContainExpectedClaims()
    {
        // Arrange: Prepares a test user and a list of roles.
        var user = new IdentityUser { UserName = "testuser", Email = "test@test.com" };
        var roles = new List<string> { "Admin", "Editor" };

        // Act: Generates the JWT token string.
        var tokenString = _sut.GenerateToken(user, roles);

        // Assert: Validates the token structure and claim values.
        var handler = new JwtSecurityTokenHandler();
        var token = handler.ReadJwtToken(tokenString);

        Assert.Equal(user.Id, token.Claims.First(c => c.Type == JwtRegisteredClaimNames.Sub).Value);
        Assert.Equal(user.Email, token.Claims.First(c => c.Type == JwtRegisteredClaimNames.Email).Value);
        Assert.Equal(user.UserName, token.Claims.First(c => c.Type == ClaimTypes.Name).Value);
        Assert.Contains(token.Claims, c => c.Type == ClaimTypes.Role && c.Value == "Admin");
        Assert.Contains(token.Claims, c => c.Type == ClaimTypes.Role && c.Value == "Editor");
        Assert.NotNull(token.Claims.First(c => c.Type == JwtRegisteredClaimNames.Jti).Value);
    }

    /// <summary>
    /// Ensures the token generation falls back to a default expiration duration if the configuration value is missing.
    /// </summary>
    [Fact]
    public void GenerateToken_WhenExpiryConfigIsMissing_ShouldUseFallbackValue()
    {
        // Arrange: Sets the expiry configuration to null to test the fallback mechanism.
        // Also ensures the user has an email to avoid ArgumentNullException during claim creation.
        _jwtSectionMock.Setup(s => s["ExpiresInMinutes"]).Returns((string)null!);
        var user = new IdentityUser { UserName = "user", Email = "user@test.com" };

        // Act: Generates the token string.
        var tokenString = _sut.GenerateToken(user, new List<string>());

        // Assert: Verifies that the expiration is set to the default fallback (approx. 60 min).
        var handler = new JwtSecurityTokenHandler();
        var token = handler.ReadJwtToken(tokenString);
        var expiry = token.ValidTo;
    
        Assert.True(expiry > DateTime.UtcNow.AddMinutes(55) && expiry < DateTime.UtcNow.AddMinutes(65));
    }

    /// <summary>
    /// Ensures the token generation falls back to a default expiration duration if the configuration value is not a valid number.
    /// </summary>
    [Fact]
    public void GenerateToken_WhenExpiryConfigIsInvalid_ShouldUseFallbackValue()
    {
        // Arrange: Provides an invalid numeric string. 
        // The IdentityUser must have an email defined to satisfy the Claim constructor requirements.
        _jwtSectionMock.Setup(s => s["ExpiresInMinutes"]).Returns("not_a_number");
        var user = new IdentityUser { UserName = "user", Email = "user@test.com" };

        // Act: Generates the token string.
        var tokenString = _sut.GenerateToken(user, new List<string>());

        // Assert: Verifies that the token was still generated using the fallback duration.
        var handler = new JwtSecurityTokenHandler();
        var token = handler.ReadJwtToken(tokenString);
        Assert.True(token.ValidTo > DateTime.UtcNow.AddMinutes(55));
    }
    
    /// <summary>
    /// The test ensures that the generated token contains the correct identity claims.
    /// It verifies that the Id is used as Sub and UserName is used as Name.
    /// </summary>
    [Fact]
    public void GenerateToken_WhenCalled_ReturnsTokenWithCorrectClaims()
    {
        // Arrange: Prepares a test user and roles.
        var user = new IdentityUser 
        { 
            Id = "user-guid-123", 
            UserName = "Cioffi", 
            Email = "henrique@test.com" 
        };
        var roles = new List<string> { "Reader", "Admin" };

        // Act: Generates the JWT token string.
        var tokenString = _sut.GenerateToken(user, roles);
        var handler = new JwtSecurityTokenHandler();
        var jwtToken = handler.ReadJwtToken(tokenString);

        // Assert: Validates the presence and value of essential claims.
        jwtToken.Claims.Should().Contain(c => c.Type == JwtRegisteredClaimNames.Sub && c.Value == "user-guid-123");
        jwtToken.Claims.Should().Contain(c => c.Type == ClaimTypes.Name && c.Value == "Cioffi");
        jwtToken.Claims.Should().Contain(c => c.Type == JwtRegisteredClaimNames.Email && c.Value == "henrique@test.com");
        jwtToken.Claims.Should().Contain(c => c.Type == ClaimTypes.Role && c.Value == "Reader");
        jwtToken.Claims.Should().Contain(c => c.Type == ClaimTypes.Role && c.Value == "Admin");
    }
}