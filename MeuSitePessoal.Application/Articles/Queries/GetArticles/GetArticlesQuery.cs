using MediatR;
using MeuSitePessoal.Application.Common.Models;
using MeuSitePessoal.Domain;
using MeuSitePessoal.Domain.Entities;

namespace MeuSitePessoal.Application.Articles.Queries.GetArticles;

/// <summary>
/// Retrieves a paginated list of article summaries, optionally filtered by multiple tags and category.
/// </summary>
public record GetArticlesQuery(int PageNumber = 1, int PageSize = 10, List<string>? Tags = null, ArticleCategory? Category = null) : IRequest<PagedResult<ArticleSummaryDto>>;