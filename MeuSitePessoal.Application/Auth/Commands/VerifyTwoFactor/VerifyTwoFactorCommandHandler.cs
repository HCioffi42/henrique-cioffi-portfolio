using MeuSitePessoal.Domain.Entities;
using MediatR;
using MeuSitePessoal.Application.Common.Interfaces;
using MeuSitePessoal.Domain.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace MeuSitePessoal.Application.Auth.Commands.VerifyTwoFactor;

/// <summary>
/// Handles the <see cref="VerifyTwoFactorCommand"/> by validating the TOTP code
/// for the specified user. Issues a JWT token upon successful verification.
/// </summary>
public class VerifyTwoFactorCommandHandler : IRequestHandler<VerifyTwoFactorCommand, VerifyTwoFactorResult>
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ITokenService _tokenService;
    private readonly ILogger<VerifyTwoFactorCommandHandler> _logger;

    /// <summary>
    /// Initializes a new instance of <see cref="VerifyTwoFactorCommandHandler"/>.
    /// </summary>
    public VerifyTwoFactorCommandHandler(
        UserManager<ApplicationUser> userManager,
        ITokenService tokenService,
        ILogger<VerifyTwoFactorCommandHandler> logger)
    {
        _userManager = userManager;
        _tokenService = tokenService;
        _logger = logger;
    }

    /// <summary>
    /// Verifies the provided TOTP code using the ASP.NET Core Identity token provider.
    /// Returns a JWT token on success, or a failure result if the code is invalid.
    /// </summary>
    /// <param name="request">The 2FA verification command.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A <see cref="VerifyTwoFactorResult"/> with the JWT token or a failure indicator.</returns>
    public async Task<VerifyTwoFactorResult> Handle(VerifyTwoFactorCommand request, CancellationToken cancellationToken)
    {
        var user = await _userManager.FindByNameAsync(request.Username);
        if (user == null)
        {
            _logger.LogWarning("2FA verify: user '{Username}' not found.", request.Username);
            return new VerifyTwoFactorResult(null, false);
        }

        var isValid = await _userManager.VerifyTwoFactorTokenAsync(
            user,
            _userManager.Options.Tokens.AuthenticatorTokenProvider,
            request.Code);

        if (!isValid)
        {
            _logger.LogWarning("Invalid 2FA code for user '{Username}'.", request.Username);
            return new VerifyTwoFactorResult(null, false);
        }

        var roles = await _userManager.GetRolesAsync(user);
        var token = _tokenService.GenerateToken(user, roles);

        _logger.LogInformation("2FA verified and token issued for user '{Username}'.", request.Username);
        return new VerifyTwoFactorResult(token, true);
    }
}

