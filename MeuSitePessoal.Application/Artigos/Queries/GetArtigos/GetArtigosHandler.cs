using MediatR;
using MeuSitePessoal.Application.Common.Models;
using MeuSitePessoal.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace MeuSitePessoal.Application.Artigos.Queries.GetArtigos;

/// <summary>
/// Handles the retrieval of paginated article summaries and applies multi-tag filtering if requested.
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
        var query = _context.Artigos.AsNoTracking();

        // Applied a loop to dynamically chain Where clauses. This creates an intersection (AND) filter.
        if (request.Tags != null && request.Tags.Any())
        {
            var validTags = request.Tags.Where(t => !string.IsNullOrWhiteSpace(t)).ToList();
            foreach (var tag in validTags)
            {
                // Captured the loop variable to prevent EF Core expression tree closure issues.
                var tagToSearch = tag; 
                query = query.Where(a => a.Tags.Contains(tagToSearch));
            }
        }

        query = query.OrderByDescending(a => a.DataCriacao);

        var totalCount = await query.CountAsync(cancellationToken);

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