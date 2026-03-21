using MediatR;

namespace MeuSitePessoal.Application.Articles.Commands.CreateArticle;

public record CreateArticleCommand(string Title, string Content, string Summary, List<string>? Tags = null)
    : IRequest<Guid>
{
    public List<string> Tags { get; init; } = Tags ?? new List<string>();
}
