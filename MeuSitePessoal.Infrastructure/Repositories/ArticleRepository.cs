using MeuSitePessoal.Domain;
using MeuSitePessoal.Domain.Entities;
using MeuSitePessoal.Domain.Interfaces;
using MeuSitePessoal.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace MeuSitePessoal.Infrastructure.Repositories;

/**
 * Implements the IArticleRepository interface using Entity Framework Core.
 * This implementation is optimized for the blog's specific data access patterns.
 */
public class ArticleRepository : IArticleRepository
{
    private readonly BlogDbContext _context;

    public ArticleRepository(BlogDbContext context)
    {
        _context = context;
    }

    /**
     * Fetches a paginated chunk of articles with AsNoTracking to optimize memory usage.
     */
    public async Task<(IEnumerable<Article> Items, int TotalCount)> GetPaginatedAsync(int pageNumber, int pageSize)
    {
        // Assuming the DbSet property is still named 'Articles' in the context
        // and the property 'CreatedAt' was renamed from 'DataCriacao'.
        var query = _context.Articles.AsNoTracking();

        var totalCount = await query.CountAsync();

        var items = await query
            .OrderByDescending(a => a.CreatedAt) 
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (items, totalCount);
    }

    /**
     * Retrieves a specific article by its Guid.
     */
    public async Task<Article?> GetByIdAsync(Guid id)
    {
        return await _context.Articles.FindAsync(id);
    }

    /**
     * Persists a new article and saves changes to the database.
     */
    public async Task AddAsync(Article article)
    {
        await _context.Articles.AddAsync(article);
        await _context.SaveChangesAsync();
    }
    
    /**
     * Removes an article based on its ID and returns the operation success.
     */
    public async Task<bool> DeleteAsync(Guid id)
    {
        var article = await _context.Articles.FindAsync(id);
        if (article == null) return false;

        _context.Articles.Remove(article);
        return await _context.SaveChangesAsync() > 0;
    }
    
    /**
     * Updates an existing article and returns the operation success.
     */
    public async Task<bool> UpdateAsync(Article article)
    {
        _context.Articles.Update(article);
        return await _context.SaveChangesAsync() > 0;
    }
}