using MediatR;
using MeuSitePessoal.Domain;

namespace MeuSitePessoal.Application.Artigos.Queries.GetArtigoById;

public record GetArtigoByIdQuery(Guid Id) : IRequest<Artigo?>;
