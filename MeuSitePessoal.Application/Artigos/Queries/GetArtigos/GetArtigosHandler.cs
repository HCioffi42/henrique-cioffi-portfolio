using MediatR;
using MeuSitePessoal.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace MeuSitePessoal.Application.Artigos.Queries.GetArtigos;

/// <summary>
/// HC: A handler that retrieves a list of article summaries directly from the database context.
/// </summary>
public class GetArtigosHandler : IRequestHandler<GetArtigosQuery, List<ArtigoSummaryDto>>
{
    private readonly BlogDbContext _context;

    public GetArtigosHandler(BlogDbContext context)
    {
        _context = context;
    }

    public async Task<List<ArtigoSummaryDto>> Handle(GetArtigosQuery request, CancellationToken cancellationToken)
    {
        // HC: Fetching only the necessary columns and ordering by creation date descending.
        // HC: Conteudo column is explicitly ignored by the projection.
        return await _context.Artigos
            .AsNoTracking()
            .OrderByDescending(a => a.DataCriacao)
            .Select(a => new ArtigoSummaryDto
            {
                Id = a.Id,
                Titulo = a.Titulo,
                Resumo = a.Resumo,
                DataCriacao = a.DataCriacao,
                Tags = a.Tags
            })
            .ToListAsync(cancellationToken);
    }
}
