using MeuSitePessoal.Domain.Entities; 
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace MeuSitePessoal.Application.Common.Interfaces;

/// <summary>
/// Defines the database contract for the application layer.
/// </summary>
public interface IBlogDbContext
{
    DbSet<Article> Articles { get; }
    DbSet<Subscriber> Subscribers { get; }
    DbSet<Comment> Comments { get; }

    // HC: Exposes the database facade to allow provider checks (like IsNpgsql)
    DatabaseFacade Database { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}