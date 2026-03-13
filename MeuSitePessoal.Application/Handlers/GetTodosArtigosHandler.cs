using MediatR;
using MeuSitePessoal.Domain;
using MeuSitePessoal.Domain.Interfaces;
using MeuSitePessoal.Application.Queries;

namespace MeuSitePessoal.Application.Handlers;

public class GetTodosArtigosHandler : IRequestHandler<GetTodosArtigosQuery, IEnumerable<Artigo>>
{
    private readonly IArtigoRepository _repository;

    public GetTodosArtigosHandler(IArtigoRepository repository)
    {
        _repository = repository;
    }

    public async Task<IEnumerable<Artigo>> Handle(GetTodosArtigosQuery request, CancellationToken cancellationToken)
    {
        return await _repository.ObterTodosAsync();
    }
}
