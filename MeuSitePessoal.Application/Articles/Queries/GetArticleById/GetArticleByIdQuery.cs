using MediatR;
using MeuSitePessoal.Domain;

namespace MeuSitePessoal.Application.Articles.Queries.GetArticleById;

public record GetArticleByIdQuery(Guid Id) : IRequest<Article?>;
