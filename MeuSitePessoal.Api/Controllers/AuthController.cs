using MediatR;
using MeuSitePessoal.Application.Auth.Commands.Login;
using MeuSitePessoal.Application.Auth.Commands.Register;
using MeuSitePessoal.Application.Auth.Commands.VerifyTwoFactor;
using MeuSitePessoal.Domain.Interfaces;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace MeuSitePessoal.Api.Controllers;

/// <summary>
/// Controller for handling authentication requests including local login,
/// user registration, two-factor verification, and external OAuth2 providers.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly SignInManager<IdentityUser> _signInManager;
    private readonly UserManager<IdentityUser> _userManager;
    private readonly ITokenService _tokenService;
    private readonly ILogger<AuthController> _logger;

    public AuthController(
        IMediator mediator,
        SignInManager<IdentityUser> signInManager,
        UserManager<IdentityUser> userManager,
        ITokenService tokenService,
        ILogger<AuthController> logger)
    {
        _mediator = mediator;
        _signInManager = signInManager;
        _userManager = userManager;
        _tokenService = tokenService;
        _logger = logger;
    }

    /// <summary>
    /// Authenticates a user with username and password.
    /// Returns a JWT token directly if 2FA is disabled, or a RequiresTwoFactor flag if enabled.
    /// </summary>
    /// <param name="request">The login request containing username and password.</param>
    /// <returns>A JWT token response or a 2FA challenge indicator.</returns>
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var result = await _mediator.Send(new LoginCommand(request.Username, request.Password));

        if (result.Token == null && !result.RequiresTwoFactor)
            return Unauthorized("Invalid credentials.");

        return Ok(new
        {
            token = result.Token,
            username = result.Username,
            requiresTwoFactor = result.RequiresTwoFactor
        });
    }

    /// <summary>
    /// Registers a new user with the Reader role.
    /// </summary>
    /// <param name="request">The registration request containing email and password.</param>
    /// <returns>200 OK on success, or 400 Bad Request with error details on failure.</returns>
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        var result = await _mediator.Send(new RegisterCommand(request.Email, request.Password));

        if (!result.Succeeded)
            return BadRequest(new { errors = result.Errors });

        return Ok(new { message = "Registration successful. You can now sign in." });
    }

    /// <summary>
    /// Verifies a TOTP code for a user who passed the initial login step with 2FA enabled.
    /// Returns a JWT token upon successful verification.
    /// </summary>
    /// <param name="request">The 2FA verification request.</param>
    /// <returns>A JWT token or 401 Unauthorized if the code is invalid.</returns>
    [HttpPost("verify-2fa")]
    public async Task<IActionResult> VerifyTwoFactor([FromBody] VerifyTwoFactorRequest request)
    {
        var result = await _mediator.Send(new VerifyTwoFactorCommand(request.Username, request.Code));

        if (!result.Succeeded)
            return Unauthorized("Invalid or expired verification code.");

        return Ok(new { token = result.Token });
    }

    /// <summary>
    /// Initiates an external OAuth2 login challenge with the specified provider (e.g., Google, GitHub).
    /// Redirects the browser to the provider's consent screen.
    /// </summary>
    /// <param name="provider">The name of the OAuth2 provider.</param>
    /// <returns>A challenge redirect to the external provider.</returns>
    [HttpGet("external-login")]
    public IActionResult ExternalLogin([FromQuery] string provider)
    {
        var redirectUrl = Url.Action(nameof(ExternalLoginCallback), "Auth");
        var properties = _signInManager.ConfigureExternalAuthenticationProperties(provider, redirectUrl);
        return Challenge(properties, provider);
    }

    /// <summary>
    /// Handles the callback from an external OAuth2 provider.
    /// Creates a new Reader account if the user does not already exist, then issues a JWT.
    /// Redirects the frontend to the home page with the token embedded as a query parameter.
    /// </summary>
    /// <returns>A redirect to the frontend with the JWT token.</returns>
    [HttpGet("external-callback")]
    public async Task<IActionResult> ExternalLoginCallback()
    {
        var info = await _signInManager.GetExternalLoginInfoAsync();
        if (info == null)
        {
            _logger.LogWarning("External login callback failed: no external login info found.");
            return BadRequest("External login failed.");
        }

        // Attempt to sign in with the existing external login link.
        var signInResult = await _signInManager.ExternalLoginSignInAsync(
            info.LoginProvider, info.ProviderKey, isPersistent: false);

        IdentityUser? user;

        if (!signInResult.Succeeded)
        {
            // The user does not exist locally — auto-provision a new Reader account.
            var email = info.Principal.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value ?? string.Empty;
            user = new IdentityUser { UserName = email, Email = email, EmailConfirmed = true };

            var createResult = await _userManager.CreateAsync(user);
            if (!createResult.Succeeded)
            {
                _logger.LogError("Failed to create external user {Email}.", email);
                return BadRequest("Unable to create account from external login.");
            }

            await _userManager.AddToRoleAsync(user, "Reader");
            await _userManager.AddLoginAsync(user, info);
            _logger.LogInformation("New Reader account auto-provisioned for external user {Email}.", email);
        }
        else
        {
            user = await _userManager.FindByLoginAsync(info.LoginProvider, info.ProviderKey);
        }

        if (user == null) return BadRequest("Unable to resolve user after external login.");

        var roles = await _userManager.GetRolesAsync(user);
        var token = _tokenService.GenerateToken(user, roles);

        // Redirects the frontend to the OAuth callback page with the token in the URL fragment.
        var frontendUrl = $"/#/oauth/callback?token={Uri.EscapeDataString(token)}&username={Uri.EscapeDataString(user.UserName ?? user.Email!)}";
        return Redirect(frontendUrl);
    }

    // --- Request Records ---

    /// <summary>The request body for the local login endpoint.</summary>
    public record LoginRequest(string Username, string Password);

    /// <summary>The request body for the registration endpoint.</summary>
    public record RegisterRequest(string Email, string Password);

    /// <summary>The request body for the 2FA verification endpoint.</summary>
    public record VerifyTwoFactorRequest(string Username, string Code);
}
