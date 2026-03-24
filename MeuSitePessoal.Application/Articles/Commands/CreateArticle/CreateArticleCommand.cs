using MediatR;
using MeuSitePessoal.Domain;

namespace MeuSitePessoal.Application.Articles.Commands.CreateArticle;

public record CreateArticleCommand(string Title, string Content, string Summary, ArticleCategory Category, List<string>? Tags = null)
    : IRequest<Guid>
{
    public List<string> Tags { get; init; } = Tags ?? new List<string>();
}
