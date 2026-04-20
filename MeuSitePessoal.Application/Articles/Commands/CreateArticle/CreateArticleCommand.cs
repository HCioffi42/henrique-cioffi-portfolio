using MediatR;
using MeuSitePessoal.Domain.Entities;

namespace MeuSitePessoal.Application.Articles.Commands.CreateArticle;

public record CreateArticleCommand(
    string TitleEn, 
    string TitlePt, 
    string ContentEn, 
    string ContentPt, 
    string SummaryEn, 
    string SummaryPt, 
    ArticleCategory Category, 
    List<string>? Tags = null)
    : IRequest<Guid>
{
    public List<string> Tags { get; init; } = Tags ?? new List<string>();
}
