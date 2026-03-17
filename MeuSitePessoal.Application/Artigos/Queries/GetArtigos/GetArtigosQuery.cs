using MediatR;
using MeuSitePessoal.Application.Common.Models;

namespace MeuSitePessoal.Application.Artigos.Queries.GetArtigos;

/// <summary>
/// Retrieves a paginated list of article summaries, optionally filtered by a specific tag.
/// </summary>
// Added the optional Tag property to the query record to support filtering.
public record GetArtigosQuery(int PageNumber = 1, int PageSize = 10, string? Tag = null) : IRequest<PagedResult<ArtigoSummaryDto>>;