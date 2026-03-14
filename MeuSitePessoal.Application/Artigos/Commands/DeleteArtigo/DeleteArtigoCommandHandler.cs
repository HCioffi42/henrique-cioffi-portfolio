using MediatR;
using MeuSitePessoal.Domain.Interfaces;

namespace MeuSitePessoal.Application.Artigos.Commands.DeleteArtigo;

public class DeleteArtigoCommandHandler : IRequestHandler<DeleteArtigoCommand, Unit>
{
    private readonly IArtigoRepository _repository;

    public DeleteArtigoCommandHandler(IArtigoRepository repository)
    {
        _repository = repository;
    }

    public async Task<Unit> Handle(DeleteArtigoCommand request, CancellationToken cancellationToken)
    {
        var artigo = await _repository.ObterPorIdAsync(request.Id);

        if (artigo == null)
        {
            throw new KeyNotFoundException($"Article with ID {request.Id} was not found.");
        }

        await _repository.ExcluirAsync(request.Id);

        return Unit.Value;
    }
}
