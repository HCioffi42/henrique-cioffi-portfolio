using MediatR;

namespace MeuSitePessoal.Application.Articles.Commands.UpdateArticle;

/**
 * Defines a positional record for updating an article.
 * The compiler automatically generates the constructor and properties.
 */
public record UpdateArticleCommand(
    Guid Id,
    string Title,
    string Content,
    string Summary,
    List<string>? Tags = null) : IRequest<bool>
{
    // Ensures that even if Tags is omitted in the constructor, 
    // the property returns an empty list instead of null.
    public List<string> Tags { get; init; } = Tags ?? new();
}