using MeuSitePessoal.Domain.Entities;
using System.Text;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;

namespace MeuSitePessoal.Application.Auth.Commands.ResetPassword;

/// <summary>
/// Handles the <see cref="ResetPasswordCommand"/> by verifying the token and setting the new password with Identity.
/// </summary>
public class ResetPasswordCommandHandler : IRequestHandler<ResetPasswordCommand, ResetPasswordResult>
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ILogger<ResetPasswordCommandHandler> _logger;

    /// <summary>
    /// Initializes a new instance of <see cref="ResetPasswordCommandHandler"/>.
    /// </summary>
    /// <param name="userManager">The Identity user manager.</param>
    /// <param name="logger">The logger instance.</param>
    public ResetPasswordCommandHandler(UserManager<ApplicationUser> userManager, ILogger<ResetPasswordCommandHandler> logger)
    {
        _userManager = userManager;
        _logger = logger;
    }

    /// <summary>
    /// Processes the password reset request by decoding the token and calling Identity.
    /// </summary>
    /// <param name="request">The command containing email, token, and new password.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A <see cref="ResetPasswordResult"/> indicating success or failure.</returns>
    public async Task<ResetPasswordResult> Handle(ResetPasswordCommand request, CancellationToken cancellationToken)
    {
        var user = await _userManager.FindByEmailAsync(request.Email);

        if (user == null)
        {
            _logger.LogWarning("Password reset attempt for non-existent mail: {Email}", request.Email);
            // We return failure here because this is the reset step where the user expects a specific result after submitting the form.
            return new ResetPasswordResult(false, new[] { "User not found." });
        }

        try
        {
            // Decode the token from Base64Url format.
            var decodedTokenBytes = WebEncoders.Base64UrlDecode(request.Token);
            var decodedToken = Encoding.UTF8.GetString(decodedTokenBytes);

            var result = await _userManager.ResetPasswordAsync(user, decodedToken, request.NewPassword);

            if (result.Succeeded)
            {
                _logger.LogInformation("Password successfully reset for user {Email}.", user.Email);
                return new ResetPasswordResult(true, Enumerable.Empty<string>());
            }

            var errors = result.Errors.Select(e => e.Description).ToList();
            _logger.LogWarning("Password reset failed for user {Email}: {Errors}", user.Email, string.Join(", ", errors));
            return new ResetPasswordResult(false, errors);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while decoding or resetting the password for user {Email}.", request.Email);
            return new ResetPasswordResult(false, new[] { "Invalid or malformed reset token." });
        }
    }
}

