using MediatR;

namespace MeuSitePessoal.Application.Commands;

public record CreateArtigoCommand(string Titulo, string Conteudo, string Resumo, List<string> Tags) : IRequest<Guid>;
