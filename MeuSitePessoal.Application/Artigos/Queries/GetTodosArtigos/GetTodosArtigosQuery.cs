using MediatR;
using MeuSitePessoal.Application.Common.Models;
using MeuSitePessoal.Domain;

namespace MeuSitePessoal.Application.Artigos.Queries.GetTodosArtigos;

// The query now accepts pagination parameters and returns a PagedList instead of a simple IEnumerable.
public record GetTodosArtigosQuery(int PageNumber = 1, int PageSize = 10) : IRequest<PagedList<Artigo>>;