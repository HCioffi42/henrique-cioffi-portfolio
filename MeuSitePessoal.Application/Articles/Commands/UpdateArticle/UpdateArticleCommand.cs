using MediatR;
using MeuSitePessoal.Domain.Entities;

namespace MeuSitePessoal.Application.Articles.Commands.UpdateArticle;

public record UpdateArticleCommand(
    Guid Id,
    string TitleEn, 
    string TitlePt, 
    string ContentEn, 
    string ContentPt, 
    string SummaryEn, 
    string SummaryPt, 
    ArticleCategory Category, 
    List<string>? Tags = null) : IRequest<bool>
{
    public List<string> Tags { get; init; } = Tags ?? new();
}