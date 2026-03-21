using MediatR;

namespace MeuSitePessoal.Application.Articles.Commands.DeleteArtigo;

/**
 * Represents a command to remove an article from the system by its unique identifier.
 */
public record DeleteArticleCommand(Guid Id) : IRequest<bool>;