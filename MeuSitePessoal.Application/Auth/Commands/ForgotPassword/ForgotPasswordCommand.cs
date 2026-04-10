using MediatR;

namespace MeuSitePessoal.Application.Auth.Commands.ForgotPassword;

/// <summary>
/// Command for initiating the password recovery process by sending a reset link via email.
/// </summary>
/// <param name="Email">The email address of the user who forgotten their password.</param>
public record ForgotPasswordCommand(string Email) : IRequest<ForgotPasswordResult>;

/// <summary>
/// Represents the result of a forgot password request.
/// </summary>
/// <param name="Succeeded">Always true to prevent account enumeration, unless a critical system failure occurs.</param>
/// <param name="Message">A generic success message for the frontend.</param>
public record ForgotPasswordResult(bool Succeeded, string Message);
