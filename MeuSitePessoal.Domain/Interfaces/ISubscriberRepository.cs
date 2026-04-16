using MeuSitePessoal.Domain.Entities;

namespace MeuSitePessoal.Domain.Interfaces;

/// <summary>
/// Interface for subscriber-related persistence operations.
/// </summary>
public interface ISubscriberRepository
{
    Task<Subscriber?> GetByIdAsync(Guid id);
    Task<Subscriber?> GetByEmailAsync(string email);
    Task<Subscriber?> GetByUnsubscribeTokenAsync(string token);
    Task<List<Subscriber>> GetActiveSubscribersAsync();
    Task AddAsync(Subscriber subscriber);
    Task UpdateAsync(Subscriber subscriber);
}
