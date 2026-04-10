using MediatR;

namespace MeuSitePessoal.Application.Auth.Commands.ConfirmEmail;

/// <summary>
/// Command for confirming a user's email address using a provided token.
/// </summary>
/// <param name="UserId">The unique identifier of the user.</param>
/// <param name="Token">The verification token received via email.</param>
public record ConfirmEmailCommand(string UserId, string Token) : IRequest<ConfirmEmailResult>;

/// <summary>
/// Represents the result of an email confirmation attempt.
/// </summary>
/// <param name="Succeeded">Indicates whether the confirmation was successful.</param>
/// <param name="Errors">A collection of error messages if confirmation failed.</param>
public record ConfirmEmailResult(bool Succeeded, IEnumerable<string> Errors);
