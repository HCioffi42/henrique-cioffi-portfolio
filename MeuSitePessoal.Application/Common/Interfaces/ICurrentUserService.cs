namespace MeuSitePessoal.Application.Common.Interfaces;

/// <summary>
/// Provides access to the current authenticated user's information.
/// </summary>
public interface ICurrentUserService
{
    /// <summary>
    /// Gets the unique identifier of the current user.
    /// Returns null if the user is not authenticated.
    /// </summary>
    string? UserId { get; }

    /// <summary>
    /// Gets a value indicating whether the current user is authenticated.
    /// </summary>
    bool IsAuthenticated { get; }

    /// <summary>
    /// Checks if the current user belongs to a specific role.
    /// </summary>
    /// <param name="role">The role name.</param>
    /// <returns>True if the user is in the role; otherwise false.</returns>
    bool IsInRole(string role);
}
