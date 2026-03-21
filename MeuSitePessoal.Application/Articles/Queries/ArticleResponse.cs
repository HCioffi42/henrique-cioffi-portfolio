namespace MeuSitePessoal.Application.Articles.Queries;

/// <summary>
/// Represents the full data of an article returned to the client.
/// </summary>
public record ArticleResponse(
    Guid Id, 
    string Title, 
    string Content, 
    string Summary, 
    DateTime CreatedAt, 
    List<string> Tags
    );