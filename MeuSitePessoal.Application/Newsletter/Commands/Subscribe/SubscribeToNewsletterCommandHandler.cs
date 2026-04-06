using MediatR;
using MeuSitePessoal.Application.Common.Models;
using MeuSitePessoal.Domain.Entities;
using MeuSitePessoal.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace MeuSitePessoal.Application.Newsletter.Commands.Subscribe;

/// <summary>
/// Handles the execution of the <see cref="SubscribeToNewsletterCommand"/> by saving the record to the persistence layer.
/// </summary>
public class SubscribeToNewsletterCommandHandler : IRequestHandler<SubscribeToNewsletterCommand, Result>
{
    private readonly BlogDbContext _dbContext;

    /// <summary>
    /// Initializes a new instance of the handler with the database context.
    /// </summary>
    public SubscribeToNewsletterCommandHandler(BlogDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    /// <summary>
    /// Processes the subscription, checking for existing active subscribers, and saving changes sequentially.
    /// </summary>
    public async Task<Result> Handle(SubscribeToNewsletterCommand request, CancellationToken cancellationToken)
    {
        var existingSubscriber = await _dbContext.Subscribers
            .FirstOrDefaultAsync(s => s.Email.ToLower() == request.Email.ToLower(), cancellationToken);

        if (existingSubscriber != null)
        {
            if (existingSubscriber.IsActive)
            {
                return Result.Failure("This email is already subscribed.", ErrorType.Conflict);
            }

            existingSubscriber.IsActive = true;
            existingSubscriber.SubscribedAt = DateTime.UtcNow;
            
            _dbContext.Subscribers.Update(existingSubscriber);
            await _dbContext.SaveChangesAsync(cancellationToken);
            
            return Result.Success();
        }

        var newSubscriber = new Subscriber
        {
            Id = Guid.NewGuid(),
            Email = request.Email.ToLower(),
            SubscribedAt = DateTime.UtcNow,
            IsActive = true
        };

        await _dbContext.Subscribers.AddAsync(newSubscriber, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
