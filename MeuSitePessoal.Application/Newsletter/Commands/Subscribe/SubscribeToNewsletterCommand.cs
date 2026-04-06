using MediatR;
using MeuSitePessoal.Application.Common.Models;

namespace MeuSitePessoal.Application.Newsletter.Commands.Subscribe;

/// <summary>
/// Represents a command to subscribe an email to the newsletter.
/// </summary>
public record SubscribeToNewsletterCommand(string Email) : IRequest<Result>;
