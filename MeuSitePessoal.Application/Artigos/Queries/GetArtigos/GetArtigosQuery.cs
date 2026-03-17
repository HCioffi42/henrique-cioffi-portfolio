using MediatR;

namespace MeuSitePessoal.Application.Artigos.Queries.GetArtigos;

/// <summary>
/// HC: A query that requests a list of article summaries.
/// </summary>
public record GetArtigosQuery : IRequest<List<ArtigoSummaryDto>>;
