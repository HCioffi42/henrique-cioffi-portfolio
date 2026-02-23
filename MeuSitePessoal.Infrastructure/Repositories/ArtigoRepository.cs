using MeuSitePessoal.Domain;
using MeuSitePessoal.Domain.Interfaces;
using MeuSitePessoal.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace MeuSitePessoal.Infrastructure.Repositories;

public class ArtigoRepository : IArtigoRepository
{
    private readonly BlogDbContext _context;

    // Initializes the repository with the database context via dependency injection.
    public ArtigoRepository(BlogDbContext context)
    {
        _context = context;
    }

    // Returns a collection of all articles stored in the PostgreSQL database.
    public async Task<IEnumerable<Artigo>> ObterTodosAsync()
    {
        return await _context.Artigos.ToListAsync();
    }

    // Searches for a single article by its primary key using the DbContext.
    public async Task<Artigo?> ObterPorIdAsync(Guid id)
    {
        return await _context.Artigos.FindAsync(id);
    }

    // Adds a new article record and saves changes to the database.
    public async Task AdicionarAsync(Artigo artigo)
    {
        await _context.Artigos.AddAsync(artigo);
        await _context.SaveChangesAsync();
    }
    
    public async Task<bool> ExcluirAsync(Guid id)
    {
        // First, we find the entity to ensure it exists before attempting deletion.
        var artigo = await _context.Artigos.FindAsync(id);
        if (artigo == null) return false;

        // Remove the entity from the context and save changes to the PostgreSQL database.
        _context.Artigos.Remove(artigo);
        await _context.SaveChangesAsync();
        return true;
    }
    
    public async Task<bool> AtualizarAsync(Artigo artigo)
    {
        // Checks if the article exists in the database before attempting to update.
        var exists = await _context.Artigos.AnyAsync(a => a.Id == artigo.Id);
        if (!exists) return false;

        // Informs the tracker that the entity has been modified.
        _context.Artigos.Update(artigo);
    
        // Persists changes to the PostgreSQL database.
        await _context.SaveChangesAsync();
        return true;
    }
}