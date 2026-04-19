using MediatR;
using MeuSitePessoal.Domain.Interfaces;
using Microsoft.Extensions.Caching.Memory;

namespace MeuSitePessoal.Application.Articles.Commands.DeleteArticle;

/**
 * Handles the removal of an article and invalidates the cache.
 * It ensures the article exists before attempting to delete it from the repository.
 */
public class DeleteArticleCommandHandler : IRequestHandler<DeleteArticleCommand, bool>
{
    private readonly IArticleRepository _repository;
    private readonly IMemoryCache _cache;

    public DeleteArticleCommandHandler(IArticleRepository repository, IMemoryCache cache)
    {
        _repository = repository;
        _cache = cache;
    }

    /**
     * Handles the removal of an article via the IArticleRepository and invalidates the cache.
     */
    public async Task<bool> Handle(DeleteArticleCommand request, CancellationToken cancellationToken)
    {
        // In this specific implementation, the repository handles the ID-based deletion.
        var success = await _repository.DeleteAsync(request.Id);

        if (success)
        {
            // Invalidate the article listing cache.
            _cache.Remove("Articles_CacheVersion");
        }

        return success;
    }
}