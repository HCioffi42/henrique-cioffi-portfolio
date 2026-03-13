using MediatR;
using MeuSitePessoal.Domain;
using MeuSitePessoal.Domain.Interfaces;
using MeuSitePessoal.Application.Queries;

namespace MeuSitePessoal.Application.Handlers;

public class GetArtigoByIdHandler : IRequestHandler<GetArtigoByIdQuery, Artigo?>
{
    private readonly IArtigoRepository _repository;

    public GetArtigoByIdHandler(IArtigoRepository repository)
    {
        _repository = repository;
    }

    public async Task<Artigo?> Handle(GetArtigoByIdQuery request, CancellationToken cancellationToken)
    {
        return await _repository.ObterPorIdAsync(request.Id);
    }
}
