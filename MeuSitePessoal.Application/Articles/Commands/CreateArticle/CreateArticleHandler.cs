using MediatR;
using MeuSitePessoal.Domain.Entities;
using MeuSitePessoal.Domain.Interfaces;
using Microsoft.Extensions.Caching.Memory;

namespace MeuSitePessoal.Application.Articles.Commands.CreateArticle;

public class CreateArticleHandler : IRequestHandler<CreateArticleCommand, Guid>
{
    private readonly IArticleRepository _repository;
    private readonly IMemoryCache _cache;
    private readonly IMediator _mediator;

    public CreateArticleHandler(IArticleRepository repository, IMemoryCache cache, IMediator mediator)
    {
        _repository = repository;
        _cache = cache;
        _mediator = mediator;
    }

    public async Task<Guid> Handle(CreateArticleCommand request, CancellationToken cancellationToken)
    {
        // HC: FIXED constructor call order. 
        // Domain Article(titleEn, titlePt, contentEn, contentPt, summaryEn, summaryPt, tags, category)
        var article = new Article(
            request.TitleEn,
            request.TitlePt,
            request.ContentEn,
            request.ContentPt,
            request.SummaryEn,
            request.SummaryPt,
            request.Tags.Select(t => t.ToLower()).ToList(),
            request.Category
        );
        
        await _repository.AddAsync(article);

        // Notify subscribers through domain event
        await _mediator.Publish(new Domain.Events.ArticlePublishedEvent(article), cancellationToken);

        _cache.Remove("Articles_CacheVersion");
        
        return article.Id;
    }
}
