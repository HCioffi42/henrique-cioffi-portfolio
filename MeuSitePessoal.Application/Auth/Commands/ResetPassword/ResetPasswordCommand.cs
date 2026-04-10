using MediatR;

namespace MeuSitePessoal.Application.Auth.Commands.ResetPassword;

/// <summary>
/// Command for resetting a user's password using a verification token.
/// </summary>
/// <param name="Email">The email address of the user.</param>
/// <param name="Token">The reset token received via email.</param>
/// <param name="NewPassword">The new password to be set for the account.</param>
public record ResetPasswordCommand(string Email, string Token, string NewPassword) : IRequest<ResetPasswordResult>;

/// <summary>
/// Represents the result of a password reset attempt.
/// </summary>
/// <param name="Succeeded">Indicates whether the reset was successful.</param>
/// <param name="Errors">A collection of error messages from Identity if the reset failed.</param>
public record ResetPasswordResult(bool Succeeded, IEnumerable<string> Errors);
