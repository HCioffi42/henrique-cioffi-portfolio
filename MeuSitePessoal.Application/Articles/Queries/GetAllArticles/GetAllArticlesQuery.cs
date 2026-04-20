using MediatR;
using MeuSitePessoal.Application.Common.Models;

namespace MeuSitePessoal.Application.Articles.Queries.GetAllArticles;

/**
 * Represents a query to retrieve a paginated list of all articles.
 * Returns ArticleResponse objects to support localized content display in Administrative UI.
 */
public record GetAllArticlesQuery(int PageNumber = 1, int PageSize = 10) : IRequest<PagedList<ArticleResponse>>;