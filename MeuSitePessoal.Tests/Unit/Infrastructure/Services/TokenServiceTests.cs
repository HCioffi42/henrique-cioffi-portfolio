using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
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
    private readonly TokenService _tokenService;

    public TokenServiceTests()
    {
        var configMock = new Mock<IConfiguration>();
        _jwtSectionMock = new Mock<IConfigurationSection>();
        _tokenService = new TokenService(configMock.Object);

        // Sets up the GetSection call to return the mocked section instead of null.
        configMock.Setup(c => c.GetSection("JwtSettings")).Returns(_jwtSectionMock.Object);

        // Configures the values within the JwtSettings section.
        _jwtSectionMock.Setup(s => s["Key"]).Returns("super_secret_key_for_testing_purposes_only_32_chars");
        _jwtSectionMock.Setup(s => s["Issuer"]).Returns("TestIssuer");
        _jwtSectionMock.Setup(s => s["Audience"]).Returns("TestAudience");
        _jwtSectionMock.Setup(s => s["ExpiresInMinutes"]).Returns("60");
    }

    [Fact]
    public void GenerateToken_ShouldContainExpectedClaims()
    {
        // Arrange: Prepares a test user and a list of roles.
        var user = new IdentityUser { UserName = "testuser", Email = "test@test.com" };
        var roles = new List<string> { "Admin", "Editor" };

        // Act: Generates the JWT token string.
        var tokenString = _tokenService.GenerateToken(user, roles);

        // Assert: Validates the token structure and claim values.
        var handler = new JwtSecurityTokenHandler();
        var token = handler.ReadJwtToken(tokenString);

        Assert.Equal("testuser", token.Claims.First(c => c.Type == JwtRegisteredClaimNames.Sub).Value);
        Assert.Equal("test@test.com", token.Claims.First(c => c.Type == JwtRegisteredClaimNames.Email).Value);
        Assert.Equal("testuser", token.Claims.First(c => c.Type == ClaimTypes.Name).Value);
        Assert.Contains(token.Claims, c => c.Type == ClaimTypes.Role && c.Value == "Admin");
        Assert.Contains(token.Claims, c => c.Type == ClaimTypes.Role && c.Value == "Editor");
        Assert.NotNull(token.Claims.First(c => c.Type == JwtRegisteredClaimNames.Jti).Value);
    }

    [Fact]
    public void GenerateToken_WhenExpiryConfigIsMissing_ShouldUseFallbackValue()
    {
        // Arrange: Sets the expiry configuration to null to test the fallback mechanism.
        // Also ensures the user has an email to avoid ArgumentNullException during claim creation.
        _jwtSectionMock.Setup(s => s["ExpiresInMinutes"]).Returns((string)null!);
        var user = new IdentityUser { UserName = "user", Email = "user@test.com" };

        // Act: Generates the token string.
        var tokenString = _tokenService.GenerateToken(user, new List<string>());

        // Assert: Verifies that the expiration is set to the default fallback (approx. 60 min).
        var handler = new JwtSecurityTokenHandler();
        var token = handler.ReadJwtToken(tokenString);
        var expiry = token.ValidTo;
    
        Assert.True(expiry > DateTime.UtcNow.AddMinutes(55) && expiry < DateTime.UtcNow.AddMinutes(65));
    }

    [Fact]
    public void GenerateToken_WhenExpiryConfigIsInvalid_ShouldUseFallbackValue()
    {
        // Arrange: Provides an invalid numeric string. 
        // The IdentityUser must have an email defined to satisfy the Claim constructor requirements.
        _jwtSectionMock.Setup(s => s["ExpiresInMinutes"]).Returns("not_a_number");
        var user = new IdentityUser { UserName = "user", Email = "user@test.com" };

        // Act: Generates the token string.
        var tokenString = _tokenService.GenerateToken(user, new List<string>());

        // Assert: Verifies that the token was still generated using the fallback duration.
        var handler = new JwtSecurityTokenHandler();
        var token = handler.ReadJwtToken(tokenString);
        Assert.True(token.ValidTo > DateTime.UtcNow.AddMinutes(55));
    }
}