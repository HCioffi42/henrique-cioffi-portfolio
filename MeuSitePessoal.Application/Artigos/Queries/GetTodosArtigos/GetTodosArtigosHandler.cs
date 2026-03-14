using MediatR;
using MeuSitePessoal.Application.Common.Models;
using MeuSitePessoal.Domain;
using MeuSitePessoal.Domain.Interfaces;

namespace MeuSitePessoal.Application.Artigos.Queries.GetTodosArtigos;

public class GetTodosArtigosHandler : IRequestHandler<GetTodosArtigosQuery, PagedList<Artigo>>
{
    private readonly IArtigoRepository _repository;

    public GetTodosArtigosHandler(IArtigoRepository repository)
    {
        _repository = repository;
    }

    public async Task<PagedList<Artigo>> Handle(GetTodosArtigosQuery request, CancellationToken cancellationToken)
    {
        // Fetches paginated items and the total count from the repository in a single flow.
        var (items, totalCount) = await _repository.ObterPaginadoAsync(request.PageNumber, request.PageSize);

        // Returns the items wrapped in the PagedList model with all pagination metadata.
        return new PagedList<Artigo>(
            items.ToList(),
            totalCount,
            request.PageNumber,
            request.PageSize);
    }
}