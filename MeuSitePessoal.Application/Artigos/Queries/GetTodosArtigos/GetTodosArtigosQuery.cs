using MediatR;
using MeuSitePessoal.Domain;

namespace MeuSitePessoal.Application.Artigos.Queries.GetTodosArtigos;

public record GetTodosArtigosQuery() : IRequest<IEnumerable<Artigo>>;
