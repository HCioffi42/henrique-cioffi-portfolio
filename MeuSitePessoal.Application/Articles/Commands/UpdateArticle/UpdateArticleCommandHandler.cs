using AutoMapper;
using MediatR;
using MeuSitePessoal.Domain.Interfaces;

namespace MeuSitePessoal.Application.Articles.Commands.UpdateArticle;

/// <summary>
/// Handles the update process for an existing article entity.
/// </summary>
public class UpdateArticleCommandHandler : IRequestHandler<UpdateArticleCommand, bool>
{
    private readonly IArticleRepository _repository;
    private readonly IMapper _mapper;

    public UpdateArticleCommandHandler(IArticleRepository repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    /// <summary>
    /// Processes the update by fetching the entity and applying changes via AutoMapper.
    /// </summary>
    public async Task<bool> Handle(UpdateArticleCommand request, CancellationToken cancellationToken)
    {
        // Fetches the existing entity from the repository using the new naming.
        var article = await _repository.GetByIdAsync(request.Id);

        if (article == null)
        {
            return false;
        }

        // Maps the command to the existing entity.
        _mapper.Map(request, article);

        // Persists changes through the updated repository method.
        return await _repository.UpdateAsync(article) ? true : throw new Exception("Failed to update the article in the database.");
    }
}