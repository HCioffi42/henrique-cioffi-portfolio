using MediatR;
using MeuSitePessoal.Application.Articles.Queries.GetArticles;

namespace MeuSitePessoal.Application.Articles.Queries.GetRelatedArticles;

/// <summary>
/// HC: Query to fetch related articles for a given article based on shared tags.
/// </summary>
/// <param name="ArticleId">The unique ID of the base article.</param>
/// <param name="Limit">The maximum number of related articles to return (default 4).</param>
public record GetRelatedArticlesQuery(Guid ArticleId, int Limit = 4) : IRequest<List<ArticleSummaryDto>>;
