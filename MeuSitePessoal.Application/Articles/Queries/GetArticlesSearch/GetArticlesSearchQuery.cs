using MediatR;
using MeuSitePessoal.Application.Common.Models;
using MeuSitePessoal.Application.Articles.Queries.GetArticles;

namespace MeuSitePessoal.Application.Articles.Queries.GetArticlesSearch;

/// <summary>
/// HC: Represents a query to search for articles using a search term.
/// </summary>
/// <param name="SearchTerm">The keyword or phrase to search for.</param>
/// <param name="PageNumber">The page number for pagination.</param>
/// <param name="PageSize">The page size for pagination.</param>
public record GetArticlesSearchQuery(string? SearchTerm, int PageNumber = 1, int PageSize = 10) 
    : IRequest<PagedResult<ArticleSummaryDto>>;
