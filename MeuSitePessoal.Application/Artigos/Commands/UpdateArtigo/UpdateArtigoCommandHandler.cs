using MediatR;
using MeuSitePessoal.Domain.Interfaces;

namespace MeuSitePessoal.Application.Artigos.Commands.UpdateArtigo;

public class UpdateArtigoCommandHandler : IRequestHandler<UpdateArtigoCommand, Unit>
{
    private readonly IArtigoRepository _repository;

    public UpdateArtigoCommandHandler(IArtigoRepository repository)
    {
        _repository = repository;
    }

    public async Task<Unit> Handle(UpdateArtigoCommand request, CancellationToken cancellationToken)
    {
        var artigo = await _repository.ObterPorIdAsync(request.Id);

        if (artigo == null)
        {
            throw new KeyNotFoundException($"Article with ID {request.Id} was not found.");
        }

        // Manual mapping from Command to existing Entity.
        // The Id is never modified as it's the primary key and immutable for this operation.
        artigo.Titulo = request.Titulo;
        artigo.Conteudo = request.Conteudo;
        artigo.Resumo = request.Resumo;
        artigo.Tags = request.Tags;

        await _repository.AtualizarAsync(artigo);

        return Unit.Value;
    }
}
