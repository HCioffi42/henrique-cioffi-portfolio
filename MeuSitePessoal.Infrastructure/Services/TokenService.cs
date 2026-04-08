using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using MeuSitePessoal.Application.Common.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace MeuSitePessoal.Infrastructure.Services;

/// <summary>
/// Service implementation for generating JWT authentication tokens.
/// </summary>
public class TokenService : ITokenService
{
    private readonly IConfiguration _configuration;

    public TokenService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    /// <summary>
    /// Generates a JWT token for the specified user and roles.
    /// </summary>
    /// <param name="user">The user to generate the token for.</param>
    /// <param name="roles">The roles of the user.</param>
    /// <returns>A JWT token string.</returns>
    public string GenerateToken(IdentityUser user, IList<string> roles)
    {
        // Retrieves JWT settings from the configuration.
        var jwtSettings = _configuration.GetSection("JwtSettings");
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings["Key"]!));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        // Maps user properties to JWT claims.
        // Uses Id for Sub/NameIdentifier to ensure a permanent unique reference.
        // Uses UserName for ClaimTypes.Name to provide the display name for the UI.
        var claims = new List<Claim>
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id),
            new Claim(JwtRegisteredClaimNames.Email, user.Email!),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim(ClaimTypes.NameIdentifier, user.Id),
            new Claim(ClaimTypes.Name, user.UserName ?? user.Email!)
        };

        // Iterates through user roles and adds each as a role claim.
        foreach (var role in roles)
        {
            claims.Add(new Claim(ClaimTypes.Role, role));
        }

        // Creates the security token descriptor and instantiates the JWT.
        var token = new JwtSecurityToken(
            issuer: jwtSettings["Issuer"],
            audience: jwtSettings["Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(double.TryParse(jwtSettings["ExpiresInMinutes"], out var minutes) ? minutes : 60),
            signingCredentials: creds
        );

        // Encodes the token into a string format.
        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
