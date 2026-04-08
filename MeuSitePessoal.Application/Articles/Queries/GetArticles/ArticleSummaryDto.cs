using MeuSitePessoal.Domain;
using MeuSitePessoal.Domain.Entities;

namespace MeuSitePessoal.Application.Articles.Queries.GetArticles;

/// <summary>
/// HC: Represeting a summary of an article for list views.
/// </summary>
public class ArticleSummaryDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Summary { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public List<string> Tags { get; set; } = new();
    public ArticleCategory Category { get; set; }
}
