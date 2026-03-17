using MediatR;
using MeuSitePessoal.Application.Common.Models;
using MeuSitePessoal.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace MeuSitePessoal.Application.Artigos.Queries.GetArtigos;

/// <summary>
/// HC: A handler that retrieves a paginated list of article summaries.
/// </summary>
public class GetArtigosHandler : IRequestHandler<GetArtigosQuery, PagedResult<ArtigoSummaryDto>>
{
    private readonly BlogDbContext _context;

    public GetArtigosHandler(BlogDbContext context)
    {
        _context = context;
    }

    public async Task<PagedResult<ArtigoSummaryDto>> Handle(GetArtigosQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Artigos
            .AsNoTracking()
            .OrderByDescending(a => a.DataCriacao);

        var totalCount = await query.CountAsync(cancellationToken);

        // HC: Fetching only the necessary columns and applying pagination.
        // HC: Conteudo column is explicitly ignored by the projection.
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
