using MediatR;
using MeuSitePessoal.Application.Common.Models;
using MeuSitePessoal.Domain;
using MeuSitePessoal.Domain.Interfaces;

namespace MeuSitePessoal.Application.Articles.Queries.GetAllArticles;

public class GetAllArticlesHandler : IRequestHandler<GetAllArticlesQuery, PagedList<Article>>
{
    private readonly IArticleRepository _repository;

    public GetAllArticlesHandler(IArticleRepository repository)
    {
        _repository = repository;
    }

    public async Task<PagedList<Article>> Handle(GetAllArticlesQuery request, CancellationToken cancellationToken)
    {
        // Fetches paginated items and the total count from the repository in a single flow.
        var (items, totalCount) = await _repository.GetPaginatedAsync(request.PageNumber, request.PageSize);

        // Returns the items wrapped in the PagedList model with all pagination metadata.
        return new PagedList<Article>(
            items.ToList(),
            totalCount,
            request.PageNumber,
            request.PageSize);
    }
}