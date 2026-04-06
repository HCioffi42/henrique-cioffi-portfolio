namespace MeuSitePessoal.Application.Auth.Commands.Login;

/// <summary>
/// Command for authenticating a user with username and password.
/// </summary>
/// <param name="Username">The username or email address of the user.</param>
/// <param name="Password">The user's password.</param>
public record LoginCommand(string Username, string Password) : MediatR.IRequest<LoginResult>;

/// <summary>
/// Represents the result of a login attempt.
/// When <see cref="RequiresTwoFactor"/> is true, the <see cref="Token"/> will be null
/// and the frontend must redirect the user to the 2FA verification step.
/// </summary>
/// <param name="Token">The JWT token if authentication is complete; otherwise null.</param>
/// <param name="Username">The authenticated username.</param>
/// <param name="RequiresTwoFactor">Indicates if a 2FA verification step is required.</param>
public record LoginResult(string? Token, string? Username, bool RequiresTwoFactor);
