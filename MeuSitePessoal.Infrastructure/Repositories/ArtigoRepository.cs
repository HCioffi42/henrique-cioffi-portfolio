using MeuSitePessoal.Domain;
using MeuSitePessoal.Domain.Interfaces;
using MeuSitePessoal.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace MeuSitePessoal.Infrastructure.Repositories;

public class ArtigoRepository : IArtigoRepository
{
    private readonly BlogDbContext _context;

    // Recebe o contexto do banco de dados via injeção de dependência.
    public ArtigoRepository(BlogDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Artigo>> ObterTodosAsync()
    {
        // Recupera a lista de artigos do PostgreSQL utilizando o Entity Framework.
        return await _context.Artigos.ToListAsync();
    }

    public async Task AdicionarAsync(Artigo artigo)
    {
        // Adiciona o objeto artigo ao contexto e persiste as mudanças no banco.
        await _context.Artigos.AddAsync(artigo);
        await _context.SaveChangesAsync();
    }
}