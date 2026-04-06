using MediatR;
using MeuSitePessoal.Domain.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace MeuSitePessoal.Application.Auth.Commands.Login;

/// <summary>
/// Handles the <see cref="LoginCommand"/> by verifying user credentials and applying
/// the 2FA feature toggle. If 2FA is disabled, the handler returns the JWT token immediately.
/// If 2FA is enabled, it returns a signal for the frontend to prompt for a TOTP code.
/// </summary>
public class LoginCommandHandler : IRequestHandler<LoginCommand, LoginResult>
{
    private readonly UserManager<IdentityUser> _userManager;
    private readonly SignInManager<IdentityUser> _signInManager;
    private readonly ITokenService _tokenService;
    private readonly IFeatureToggleService _featureToggle;
    private readonly ILogger<LoginCommandHandler> _logger;

    /// <summary>
    /// Initializes a new instance of <see cref="LoginCommandHandler"/>.
    /// </summary>
    public LoginCommandHandler(
        UserManager<IdentityUser> userManager,
        SignInManager<IdentityUser> signInManager,
        ITokenService tokenService,
        IFeatureToggleService featureToggle,
        ILogger<LoginCommandHandler> logger)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _tokenService = tokenService;
        _featureToggle = featureToggle;
        _logger = logger;
    }

    /// <summary>
    /// Validates credentials, then either returns a JWT token directly or signals that
    /// a 2FA step is required, based on the <c>FeatureToggles:Enable2FA</c> configuration flag.
    /// </summary>
    /// <param name="request">The login command.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A <see cref="LoginResult"/> with a token or a RequiresTwoFactor flag.</returns>
    public async Task<LoginResult> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        var user = await _userManager.FindByNameAsync(request.Username);
        if (user == null)
        {
            _logger.LogWarning("Login failed: user '{Username}' not found.", request.Username);
            return new LoginResult(null, null, false);
        }

        var result = await _signInManager.CheckPasswordSignInAsync(user, request.Password, lockoutOnFailure: false);
        if (!result.Succeeded)
        {
            _logger.LogWarning("Login failed: incorrect password for user '{Username}'.", request.Username);
            return new LoginResult(null, null, false);
        }

        // The 2FA toggle determines the next step after successful password validation.
        if (_featureToggle.Is2FAEnabled())
        {
            _logger.LogInformation("2FA required for user '{Username}'.", request.Username);
            return new LoginResult(null, user.UserName, RequiresTwoFactor: true);
        }

        // 2FA is disabled — issue a token immediately.
        var roles = await _userManager.GetRolesAsync(user);
        var token = _tokenService.GenerateToken(user, roles);

        _logger.LogInformation("Successful login for user '{Username}'.", request.Username);
        return new LoginResult(token, user.UserName, RequiresTwoFactor: false);
    }
}
