using MediatR;
using MeuSitePessoal.Application.Common.Models;

namespace MeuSitePessoal.Application.Artigos.Queries.GetArtigos;

/// <summary>
/// Retrieves a paginated list of article summaries, optionally filtered by multiple tags.
/// </summary>
public record GetArtigosQuery(int PageNumber = 1, int PageSize = 10, List<string>? Tags = null) : IRequest<PagedResult<ArtigoSummaryDto>>;