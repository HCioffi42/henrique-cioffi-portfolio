using MediatR;
using MeuSitePessoal.Domain;
using MeuSitePessoal.Domain.Interfaces;
using MeuSitePessoal.Application.Commands;

namespace MeuSitePessoal.Application.Handlers;

public class CreateArtigoHandler : IRequestHandler<CreateArtigoCommand, Guid>
{
    private readonly IArtigoRepository _repository;

    public CreateArtigoHandler(IArtigoRepository repository)
    {
        _repository = repository;
    }

    public async Task<Guid> Handle(CreateArtigoCommand request, CancellationToken cancellationToken)
    {
        var artigo = new Artigo(request.Titulo, request.Conteudo, request.Resumo, request.Tags);
        
        await _repository.AdicionarAsync(artigo);
        
        return artigo.Id;
    }
}
