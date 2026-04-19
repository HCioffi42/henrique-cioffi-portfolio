using MeuSitePessoal.Domain.Entities;
using Microsoft.AspNetCore.Identity;

namespace MeuSitePessoal.Application.Common.Interfaces;

/// <summary>
/// Service interface for generating authentication tokens.
/// </summary>
public interface ITokenService
{
    /// <summary>
    /// Generates a JWT token for the specified user and roles.
    /// </summary>
    /// <param name="user">The user to generate the token for.</param>
    /// <param name="roles">The roles of the user.</param>
    /// <returns>A JWT token string.</returns>
    string GenerateToken(ApplicationUser user, IList<string> roles);
}

