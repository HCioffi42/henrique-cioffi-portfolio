using MeuSitePessoal.Domain.Entities;
using System.Text;
using MediatR;
using MeuSitePessoal.Domain.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;

namespace MeuSitePessoal.Application.Auth.Commands.ConfirmEmail;

/// <summary>
/// Handles the <see cref="ConfirmEmailCommand"/> by verifying the token with Identity.
/// </summary>
public class ConfirmEmailCommandHandler : IRequestHandler<ConfirmEmailCommand, ConfirmEmailResult>
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ILogger<ConfirmEmailCommandHandler> _logger;
    private readonly ISubscriberRepository _subscriberRepository;

    /// <summary>
    /// Initializes a new instance of <see cref="ConfirmEmailCommandHandler"/>.
    /// </summary>
    public ConfirmEmailCommandHandler(
        UserManager<ApplicationUser> userManager, 
        ILogger<ConfirmEmailCommandHandler> logger,
        ISubscriberRepository subscriberRepository)
    {
        _userManager = userManager;
        _logger = logger;
        _subscriberRepository = subscriberRepository;
    }

    /// <summary>
    /// Processes the email confirmation by decoding the token and calling Identity.
    /// </summary>
    /// <param name="request">The command containing UserId and Token.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A <see cref="ConfirmEmailResult"/> indicating success or failure.</returns>
    public async Task<ConfirmEmailResult> Handle(ConfirmEmailCommand request, CancellationToken cancellationToken)
    {
        var user = await _userManager.FindByIdAsync(request.UserId);

        if (user == null)
        {
            _logger.LogWarning("Email confirmation attempt for non-existent user ID: {UserId}", request.UserId);
            return new ConfirmEmailResult(false, new[] { "User not found." });
        }

        try
        {
            // Decode the token from Base64Url format.
            var decodedTokenBytes = WebEncoders.Base64UrlDecode(request.Token);
            var decodedToken = Encoding.UTF8.GetString(decodedTokenBytes);

            var result = await _userManager.ConfirmEmailAsync(user, decodedToken);

            if (result.Succeeded)
            {
                _logger.LogInformation("Email successfully confirmed for user {Email}.", user.Email);

                // HC: Also mark the corresponding newsletter subscriber as verified.
                if (user.Email != null)
                {
                    var subscriber = await _subscriberRepository.GetByEmailAsync(user.Email);
                    if (subscriber != null)
                    {
                        subscriber.IsVerified = true;
                        subscriber.VerifiedAt = DateTime.UtcNow;
                        await _subscriberRepository.UpdateAsync(subscriber);
                        _logger.LogInformation("Newsletter subscription verified for {Email}.", user.Email);
                    }
                }

                return new ConfirmEmailResult(true, Enumerable.Empty<string>());
            }

            var errors = result.Errors.Select(e => e.Description).ToList();
            _logger.LogWarning("Email confirmation failed for user {Email}: {Errors}", user.Email, string.Join(", ", errors));
            return new ConfirmEmailResult(false, errors);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while decoding or confirming the email for user {UserId}.", request.UserId);
            return new ConfirmEmailResult(false, new[] { "Invalid token format." });
        }
    }
}

