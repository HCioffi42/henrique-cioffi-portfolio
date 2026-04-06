namespace MeuSitePessoal.Application.Auth.Commands.VerifyTwoFactor;

/// <summary>
/// Command for verifying a TOTP code after the initial login step signals 2FA is required.
/// </summary>
/// <param name="Username">The username of the user verifying their 2FA code.</param>
/// <param name="Code">The 6-digit TOTP code from the user's authenticator app.</param>
public record VerifyTwoFactorCommand(string Username, string Code) : MediatR.IRequest<VerifyTwoFactorResult>;

/// <summary>
/// Represents the result of a 2FA verification attempt.
/// </summary>
/// <param name="Token">The JWT token if verification succeeded; otherwise null.</param>
/// <param name="Succeeded">Indicates whether the code was valid.</param>
public record VerifyTwoFactorResult(string? Token, bool Succeeded);
