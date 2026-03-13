using MediatR;
using MeuSitePessoal.Domain;

namespace MeuSitePessoal.Application.Queries;

public record GetTodosArtigosQuery() : IRequest<IEnumerable<Artigo>>;
