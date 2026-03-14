using MediatR;

namespace MeuSitePessoal.Application.Artigos.Commands.DeleteArtigo;

public record DeleteArtigoCommand(Guid Id) : IRequest<Unit>;
