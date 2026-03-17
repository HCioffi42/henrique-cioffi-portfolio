using MediatR;
using MeuSitePessoal.Application.Common.Models;
using MeuSitePessoal.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace MeuSitePessoal.Application.Artigos.Queries.GetArtigos;

/// <summary>
/// Handles the retrieval of paginated article summaries and applies tag filtering if requested.
/// </summary>
public class GetArtigosHandler : IRequestHandler<GetArtigosQuery, PagedResult<ArtigoSummaryDto>>
{
    private readonly BlogDbContext _context;

    /// <summary>
    /// Initializes a new instance of the handler with the injected database context.
    /// </summary>
    public GetArtigosHandler(BlogDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Processes the query by filtering, paginating, and projecting the article summaries.
    /// </summary>
    public async Task<PagedResult<ArtigoSummaryDto>> Handle(GetArtigosQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Artigos
            .AsNoTracking();

        // Applies the tag filter if a tag is provided in the request.
        if (!string.IsNullOrWhiteSpace(request.Tag))
        {
            query = query.Where(a => a.Tags.Contains(request.Tag));
        }

        // Orders the filtered results by creation date to maintain chronological display.
        query = query.OrderByDescending(a => a.DataCriacao);

        // Calculates the total count based on the filtered query, ensuring accurate pagination metadata.
        var totalCount = await query.CountAsync(cancellationToken);

        // Fetches only the necessary columns and applies pagination limits.
        // The heavy 'Conteudo' column is explicitly ignored by the projection.
        var items = await query
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(a => new ArtigoSummaryDto
            {
                Id = a.Id,
                Titulo = a.Titulo,
                Resumo = a.Resumo,
                DataCriacao = a.DataCriacao,
                Tags = a.Tags
            })
            .ToListAsync(cancellationToken);

        return new PagedResult<ArtigoSummaryDto>(items, totalCount, request.PageNumber, request.PageSize);
    }
}