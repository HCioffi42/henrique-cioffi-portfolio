using MediatR;
using MeuSitePessoal.Application.Common.Interfaces;
using MeuSitePessoal.Application.Common.Models;
using Microsoft.EntityFrameworkCore;

namespace MeuSitePessoal.Application.Newsletter.Commands.ConfirmSubscription;

/// <summary>
/// Handles the confirmation of a newsletter subscription.
/// </summary>
public class ConfirmSubscriptionCommandHandler : IRequestHandler<ConfirmSubscriptionCommand, Result>
{
    private readonly IBlogDbContext _dbContext;

    /// <summary>
    /// Initializes a new instance of the handler with the database context.
    /// </summary>
    public ConfirmSubscriptionCommandHandler(IBlogDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    /// <summary>
    /// Validates the token for the given email and activates the subscriber.
    /// </summary>
    public async Task<Result> Handle(ConfirmSubscriptionCommand request, CancellationToken cancellationToken)
    {
        var normalizedEmail = request.Email.ToLower();
        var subscriber = await _dbContext.Subscribers
            .FirstOrDefaultAsync(s => s.Email == normalizedEmail, cancellationToken);

        if (subscriber == null)
        {
            return Result.Failure("Subscriber not found.", ErrorType.NotFound);
        }

        if (subscriber.IsVerified)
        {
            return Result.Success();
        }

        if (subscriber.VerificationToken != request.Token)
        {
            return Result.Failure("Invalid verification token.", ErrorType.Failure);
        }

        // HC: Activate and verify the subscriber.
        subscriber.IsVerified = true;
        subscriber.IsActive = true;
        subscriber.VerifiedAt = DateTime.UtcNow;
        subscriber.VerificationToken = null; // Clear token after use

        _dbContext.Subscribers.Update(subscriber);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
