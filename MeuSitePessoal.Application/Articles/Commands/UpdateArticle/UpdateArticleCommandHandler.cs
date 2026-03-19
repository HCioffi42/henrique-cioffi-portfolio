using AutoMapper;
using MediatR;
using MeuSitePessoal.Domain.Interfaces;

namespace MeuSitePessoal.Application.Articles.Commands.UpdateArticle;

/**
 * Handles the logic for updating an article.
 * It fetches the existing entity, maps new values, and persists changes.
 */
public class UpdateArticleCommandHandler : IRequestHandler<UpdateArticleCommand, bool>
{
    private readonly IArticleRepository _repository;
    private readonly IMapper _mapper;

    public UpdateArticleCommandHandler(IArticleRepository repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    /**
 * Handles the logic for updating an article using the English naming conventions.
 */
    public async Task<bool> Handle(UpdateArticleCommand request, CancellationToken cancellationToken)
    {
        // Fetches the existing entity from the repository using the new naming.
        var article = await _repository.GetByIdAsync(request.Id);

        if (article == null)
        {
            // throw new Exception($"Article {request.Id} not found.");
            return false;
        }

        // Maps the command to the existing entity.
        _mapper.Map(request, article);

        // Persists changes through the updated repository method.
        var success = await _repository.UpdateAsync(article);

        return success ? true : throw new Exception("Failed to update the article in the database.");
    }
}