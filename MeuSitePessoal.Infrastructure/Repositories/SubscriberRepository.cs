using MeuSitePessoal.Domain.Entities;
using MeuSitePessoal.Domain.Interfaces;
using MeuSitePessoal.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace MeuSitePessoal.Infrastructure.Repositories;

/// <summary>
/// Implements the ISubscriberRepository interface using Entity Framework Core.
/// </summary>
public class SubscriberRepository : ISubscriberRepository
{
    private readonly BlogDbContext _context;

    public SubscriberRepository(BlogDbContext context)
    {
        _context = context;
    }

    public async Task<Subscriber?> GetByIdAsync(Guid id)
    {
        return await _context.Subscribers.FindAsync(id);
    }

    public async Task<Subscriber?> GetByEmailAsync(string email)
    {
        return await _context.Subscribers.FirstOrDefaultAsync(s => s.Email == email);
    }

    public async Task<Subscriber?> GetByUnsubscribeTokenAsync(string token)
    {
        return await _context.Subscribers.FirstOrDefaultAsync(s => s.UnsubscribeToken == token);
    }

    public async Task<List<Subscriber>> GetActiveSubscribersAsync()
    {
        return await _context.Subscribers
            .Where(s => s.IsActive && s.IsVerified)
            .ToListAsync();
    }

    public async Task AddAsync(Subscriber subscriber)
    {
        await _context.Subscribers.AddAsync(subscriber);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(Subscriber subscriber)
    {
        _context.Subscribers.Update(subscriber);
        await _context.SaveChangesAsync();
    }
}
