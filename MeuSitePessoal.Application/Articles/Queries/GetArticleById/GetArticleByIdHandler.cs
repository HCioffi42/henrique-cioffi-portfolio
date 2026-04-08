using MediatR;
using MeuSitePessoal.Domain;
using MeuSitePessoal.Domain.Entities;
using MeuSitePessoal.Domain.Interfaces;

namespace MeuSitePessoal.Application.Articles.Queries.GetArticleById;

public class GetArticleByIdHandler : IRequestHandler<GetArticleByIdQuery, Article?>
{
    private readonly IArticleRepository _repository;

    public GetArticleByIdHandler(IArticleRepository repository)
    {
        _repository = repository;
    }

    public async Task<Article?> Handle(GetArticleByIdQuery request, CancellationToken cancellationToken)
    {
        return await _repository.GetByIdAsync(request.Id);
    }
}
