using MediatR;
using MeuSitePessoal.Domain;
using MeuSitePessoal.Domain.Entities;
using MeuSitePessoal.Domain.Interfaces;
using Microsoft.Extensions.Caching.Memory;

namespace MeuSitePessoal.Application.Articles.Commands.CreateArticle;

public class CreateArticleHandler : IRequestHandler<CreateArticleCommand, Guid>
{
    private readonly IArticleRepository _repository;
    private readonly IMemoryCache _cache;

    // Initializes the handler with the necessary repository and cache through dependency injection.
    public CreateArticleHandler(IArticleRepository repository, IMemoryCache cache)
    {
        _repository = repository;
        _cache = cache;
    }

    // Processes the article creation request by mapping command data to a new domain entity and invalidates the cache.
    public async Task<Guid> Handle(CreateArticleCommand request, CancellationToken cancellationToken)
    {
        // Creates a new Article instance with the provided Title, Content, Summary, and Tags.
        var artigo = new Article(
            request.Title, 
            request.Content, 
            request.Summary, 
            request.Tags.Select(t => t.ToLower()).ToList(), // Normalizes tags to lowercase for consistent storage and querying.
            request.Category
        );
        
        // Persists the article entity into the PostgreSQL database via the infrastructure layer.
        await _repository.AddAsync(artigo);

        // Invalidate the cache by removing a common key or using a strategy to force refresh.
        // For simplicity in this track, we use a basic approach that would work with prefix-based invalidation if we had it.
        // Since we don't have a built-in "clear by prefix", we can use a "CacheVersion" pattern or just let it expire.
        // For now, we will use a "last update" key that the query handler can check, or just accept that 
        // a more robust invalidation (like using a CancellationChangeToken) would be better.
        // To strictly fulfill "Ensure the cache is invalidated", we'll use a versioning approach in the Query Handler.
        _cache.Remove("Articles_CacheVersion");
        
        // Returns the unique identifier assigned to the newly created article.
        return artigo.Id;
    }
}
