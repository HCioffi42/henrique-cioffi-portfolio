namespace MeuSitePessoal.Application.Auth.Commands.Login;

/// <summary>
/// Command for authenticating a user with username and password.
/// </summary>
/// <param name="Username">The username or email address of the user.</param>
/// <param name="Password">The user's password.</param>
public record LoginCommand(string Username, string Password) : MediatR.IRequest<LoginResult>;

/// <summary>
/// Represents the result of a login attempt. A failed attempt carries a null
/// <see cref="Token"/> and a null <see cref="Username"/>.
/// </summary>
/// <param name="Token">The JWT token if authentication succeeded; otherwise null.</param>
/// <param name="Username">The authenticated username.</param>
public record LoginResult(string? Token, string? Username);
