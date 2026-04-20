namespace MeuSitePessoal.Application.Articles.Queries;

/// <summary>
/// Represents the full data of an article returned to the client.
/// </summary>
public record ArticleResponse(
    Guid Id, 
    string Title, 
    string TitleEn,
    string TitlePt,
    string Content, 
    string ContentEn,
    string ContentPt,
    string Summary, 
    string SummaryEn,
    string SummaryPt,
    DateTime CreatedAt, 
    List<string> Tags,
    Domain.Entities.ArticleCategory Category
    );