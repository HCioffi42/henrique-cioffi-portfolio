using MeuSitePessoal.Domain;
using MeuSitePessoal.Domain.Interfaces;
using MeuSitePessoal.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace MeuSitePessoal.Infrastructure.Repositories;

public class ArtigoRepository : IArtigoRepository
{
    private readonly BlogDbContext _context;

    public ArtigoRepository(BlogDbContext context)
    {
        _context = context;
    }

    // Returns all articles without tracking for better performance in read-only queries.
    public async Task<IEnumerable<Artigo>> ObterTodosAsync()
    {
        return await _context.Artigos.AsNoTracking().ToListAsync();
    }

    public async Task<Artigo?> ObterPorIdAsync(Guid id)
    {
        return await _context.Artigos.FindAsync(id);
    }

    public async Task AdicionarAsync(Artigo artigo)
    {
        await _context.Artigos.AddAsync(artigo);
        await _context.SaveChangesAsync();
    }
    
    public async Task<bool> ExcluirAsync(Guid id)
    {
        // We fetch the article here. If the Handler already did it, EF Core will use the local tracker.
        var artigo = await _context.Artigos.FindAsync(id);
        if (artigo == null) return false;

        _context.Artigos.Remove(artigo);
        return await _context.SaveChangesAsync() > 0;
    }
    
    public async Task<bool> AtualizarAsync(Artigo artigo)
    {
        // Since the Handler will verify existence, we can use the tracker directly.
        _context.Artigos.Update(artigo);
        return await _context.SaveChangesAsync() > 0;
    }
}