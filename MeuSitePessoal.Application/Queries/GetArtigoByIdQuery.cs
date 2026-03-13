using MediatR;
using MeuSitePessoal.Domain;

namespace MeuSitePessoal.Application.Queries;

public record GetArtigoByIdQuery(Guid Id) : IRequest<Artigo?>;
