using MediatR;
using MeuSitePessoal.Application.Common.Models;
using MeuSitePessoal.Domain;
using MeuSitePessoal.Domain.Entities;

namespace MeuSitePessoal.Application.Articles.Queries.GetAllArticles;

// The query now accepts pagination parameters and returns a PagedList instead of a simple IEnumerable.
public record GetAllArticlesQuery(int PageNumber = 1, int PageSize = 10) : IRequest<PagedList<Article>>;