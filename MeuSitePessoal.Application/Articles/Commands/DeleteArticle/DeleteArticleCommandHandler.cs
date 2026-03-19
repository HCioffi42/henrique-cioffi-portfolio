using MediatR;
using MeuSitePessoal.Domain.Interfaces;

namespace MeuSitePessoal.Application.Articles.Commands.DeleteArtigo;

/**
 * Handles the removal of an article.
 * It ensures the article exists before attempting to delete it from the repository.
 */
public class DeleteArticleCommandHandler : IRequestHandler<DeleteArticleCommand, bool>
{
    private readonly IArticleRepository _repository;

    public DeleteArticleCommandHandler(IArticleRepository repository)
    {
        _repository = repository;
    }

    /**
     * Handles the removal of an article via the IArticleRepository.
     */
    public async Task<bool> Handle(DeleteArticleCommand request, CancellationToken cancellationToken)
    {
        // In this specific implementation, the repository handles the ID-based deletion.
        return await _repository.DeleteAsync(request.Id);
    }
}