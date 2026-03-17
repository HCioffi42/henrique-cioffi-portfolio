using MediatR;
using MeuSitePessoal.Application.Common.Models;

namespace MeuSitePessoal.Application.Artigos.Queries.GetArtigos;

/// <summary>
/// HC: A query that requests a paginated list of article summaries.
/// </summary>
public record GetArtigosQuery(int PageNumber = 1, int PageSize = 10) : IRequest<PagedResult<ArtigoSummaryDto>>;
