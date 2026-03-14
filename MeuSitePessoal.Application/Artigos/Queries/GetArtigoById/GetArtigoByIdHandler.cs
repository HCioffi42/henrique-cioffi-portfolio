using MediatR;
using MeuSitePessoal.Domain;
using MeuSitePessoal.Domain.Interfaces;

namespace MeuSitePessoal.Application.Artigos.Queries.GetArtigoById;

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
