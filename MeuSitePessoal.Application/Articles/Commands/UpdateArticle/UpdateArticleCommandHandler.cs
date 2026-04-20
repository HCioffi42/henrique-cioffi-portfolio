using AutoMapper;
using MediatR;
using MeuSitePessoal.Domain.Interfaces;
using Microsoft.Extensions.Caching.Memory;

namespace MeuSitePessoal.Application.Articles.Commands.UpdateArticle;

/// <summary>
/// Handles the update process for an existing article entity and invalidates the cache.
/// </summary>
public class UpdateArticleCommandHandler : IRequestHandler<UpdateArticleCommand, bool>
{
    private readonly IArticleRepository _repository;
    private readonly IMapper _mapper;
    private readonly IMemoryCache _cache;

    public UpdateArticleCommandHandler(IArticleRepository repository, IMapper mapper, IMemoryCache cache)
    {
        _repository = repository;
        _mapper = mapper;
        _cache = cache;
    }

    /// <summary>
    /// Processes the update by fetching the entity, applying changes via AutoMapper, and invalidating the cache.
    /// </summary>
    public async Task<bool> Handle(UpdateArticleCommand request, CancellationToken cancellationToken)
    {
        var article = await _repository.GetByIdAsync(request.Id);

        if (article == null)
        {
            return false;
        }

        // Maps the command to the existing entity.
        // AutoMapper will handle the mapping of localized fields (TitleEn, TitlePt, etc.).
        _mapper.Map(request, article);

        // HC: STOPPED mirroring to legacy columns for new/updated records as requested.
        // We keep the legacy columns in the database for now, but we don't write to them anymore.
        // This ensures the "clean transition" where new data only exists in the new columns.

        var success = await _repository.UpdateAsync(article);
        
        if (success)
        {
            _cache.Remove("Articles_CacheVersion");
        }
        else
        {
            throw new Exception("Failed to update the article in the database.");
        }

        return true;
    }
}