using MediatR;
using MeuSitePessoal.Application.Common.Models;

namespace MeuSitePessoal.Application.Newsletter.Commands.Unsubscribe;

/// <summary>
/// Command to unsubscribe a user from the newsletter using their email and a security token.
/// </summary>
public record UnsubscribeCommand(string Email, string Token) : IRequest<Result<bool>>;
