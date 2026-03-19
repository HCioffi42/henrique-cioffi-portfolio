using MediatR;
using MeuSitePessoal.Application.Common.Models;

namespace MeuSitePessoal.Application.Articles.Queries.GetArticles;

/// <summary>
/// Retrieves a paginated list of article summaries, optionally filtered by multiple tags.
/// </summary>
public record GetArticlesQuery(int PageNumber = 1, int PageSize = 10, List<string>? Tags = null) : IRequest<PagedResult<ArticleSummaryDto>>;