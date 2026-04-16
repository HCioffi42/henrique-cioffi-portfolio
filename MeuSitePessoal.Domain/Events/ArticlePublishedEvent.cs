using MediatR;
using MeuSitePessoal.Domain.Entities;

namespace MeuSitePessoal.Domain.Events;

/// <summary>
/// Domain event triggered when a new article is successfully published.
/// </summary>
public class ArticlePublishedEvent : INotification
{
    public Article Article { get; }

    public ArticlePublishedEvent(Article article)
    {
        Article = article;
    }
}
