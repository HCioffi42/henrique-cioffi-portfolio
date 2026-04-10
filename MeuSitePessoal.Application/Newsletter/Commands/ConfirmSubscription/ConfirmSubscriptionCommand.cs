using MediatR;
using MeuSitePessoal.Application.Common.Models;

namespace MeuSitePessoal.Application.Newsletter.Commands.ConfirmSubscription;

/// <summary>
/// Command to confirm a newsletter subscription using a verification token.
/// </summary>
public record ConfirmSubscriptionCommand(string Email, string Token) : IRequest<Result>;
