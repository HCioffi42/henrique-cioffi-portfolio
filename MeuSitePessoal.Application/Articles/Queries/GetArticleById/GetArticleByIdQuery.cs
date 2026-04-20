using MediatR;
using MeuSitePessoal.Domain;
using MeuSitePessoal.Domain.Entities;

namespace MeuSitePessoal.Application.Articles.Queries.GetArticleById;

public record GetArticleByIdQuery(Guid Id) : IRequest<ArticleResponse?>;
