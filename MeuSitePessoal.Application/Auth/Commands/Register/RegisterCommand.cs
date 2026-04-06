namespace MeuSitePessoal.Application.Auth.Commands.Register;

/// <summary>
/// Command for registering a new Reader user in the system.
/// </summary>
/// <param name="Email">The email address of the new user.</param>
/// <param name="Password">The password for the new user account.</param>
public record RegisterCommand(string UserName, string Email, string Password) : MediatR.IRequest<RegisterResult>;

/// <summary>
/// Represents the result of a registration attempt.
/// </summary>
/// <param name="Succeeded">Indicates whether the registration was successful.</param>
/// <param name="Errors">A collection of error messages if registration failed.</param>
public record RegisterResult(bool Succeeded, IEnumerable<string> Errors);
