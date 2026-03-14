using MediatR;

namespace MeuSitePessoal.Application.Artigos.Commands.UpdateArtigo;

public record UpdateArtigoCommand(Guid Id, string Titulo, string Conteudo, string Resumo, List<string>? Tags = null)
    : IRequest<Unit>
{
    public List<string> Tags { get; init; } = Tags ?? new List<string>();
}
