using MediatR;
using MeuSitePessoal.Application.Common.Models;
using MeuSitePessoal.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace MeuSitePessoal.Application.Newsletter.Commands.Unsubscribe;

/// <summary>
/// Handles the UnsubscribeCommand by finding the subscriber and setting IsActive = false.
/// </summary>
public class UnsubscribeHandler : IRequestHandler<UnsubscribeCommand, Result<bool>>
{
    private readonly ISubscriberRepository _subscriberRepository;
    private readonly ILogger<UnsubscribeHandler> _logger;

    public UnsubscribeHandler(ISubscriberRepository subscriberRepository, ILogger<UnsubscribeHandler> logger)
    {
        _subscriberRepository = subscriberRepository;
        _logger = logger;
    }

    public async Task<Result<bool>> Handle(UnsubscribeCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Attempting to unsubscribe: {Email}", request.Email);

        var subscriber = await _subscriberRepository.GetByEmailAsync(request.Email);

        if (subscriber == null)
        {
            _logger.LogWarning("Subscriber not found for email: {Email}", request.Email);
            return Result.Failure<bool>("Subscriber not found.", ErrorType.NotFound);
        }

        if (subscriber.UnsubscribeToken != request.Token)
        {
            _logger.LogWarning("Invalid unsubscribe token for email: {Email}", request.Email);
            return Result.Failure<bool>("Invalid unsubscribe token.");
        }

        if (!subscriber.IsActive)
        {
            _logger.LogInformation("Subscriber already inactive: {Email}", request.Email);
            return Result.Success(true);
        }

        subscriber.IsActive = false;
        await _subscriberRepository.UpdateAsync(subscriber);

        _logger.LogInformation("Successfully unsubscribed: {Email}", request.Email);
        return Result.Success(true);
    }
}
