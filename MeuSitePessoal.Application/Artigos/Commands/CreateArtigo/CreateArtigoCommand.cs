using MediatR;

namespace MeuSitePessoal.Application.Artigos.Commands.CreateArtigo;

public record CreateArtigoCommand(string Titulo, string Conteudo, string Resumo, List<string>? Tags = null)
    : IRequest<Guid>
{
    public List<string> Tags { get; init; } = Tags ?? new List<string>();
}
