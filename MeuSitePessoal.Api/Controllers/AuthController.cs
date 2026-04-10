using System.Security.Claims;
using MediatR;
using MeuSitePessoal.Application.Auth.Commands.ConfirmEmail;
using MeuSitePessoal.Application.Auth.Commands.ForgotPassword;
using MeuSitePessoal.Application.Auth.Commands.Login;
using MeuSitePessoal.Application.Auth.Commands.Register;
using MeuSitePessoal.Application.Auth.Commands.ResetPassword;
using MeuSitePessoal.Application.Auth.Commands.VerifyTwoFactor;

using MeuSitePessoal.Application.Common.Interfaces;
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
        var result = await _mediator.Send(new RegisterCommand(request.UserName, request.Email, request.Password));

        if (!result.Succeeded)
            return BadRequest(new { errors = result.Errors });

        return Ok(new { message = "Registration successful. Please check your email to confirm your account." });
    }

    /// <summary>
    /// Confirms a user's email address using the provided user ID and verification token.
    /// </summary>
    /// <param name="userId">The ID of the user confirming their email.</param>
    /// <param name="token">The verification token.</param>
    /// <returns>200 OK on success, or 400 Bad Request on failure.</returns>
    [HttpGet("confirm-email")]
    public async Task<IActionResult> ConfirmEmail([FromQuery] string userId, [FromQuery] string token)
    {
        var result = await _mediator.Send(new ConfirmEmailCommand(userId, token));

        if (!result.Succeeded)
            return BadRequest(new { errors = result.Errors });

        return Ok(new { message = "Email confirmed successfully. You can now sign in." });
    }

    /// <summary>
    /// Initiates the password recovery process by sending a reset link to the user's email.
    /// Returns a generic success message regardless of existence to prevent account enumeration.
    /// </summary>
    /// <param name="request">The request containing the user's email.</param>
    /// <returns>A generic success message.</returns>
    [HttpPost("forgot-password")]
    public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequest request)
    {
        var result = await _mediator.Send(new ForgotPasswordCommand(request.Email));
        return Ok(new { message = result.Message });
    }

    /// <summary>
    /// Resets a user's password using the provided email, token, and new password.
    /// </summary>
    /// <param name="request">The reset request details.</param>
    /// <returns>200 OK on success, or 400 Bad Request if the token is invalid or expired.</returns>
    [HttpPost("reset-password")]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest request)
    {
        var result = await _mediator.Send(new ResetPasswordCommand(request.Email, request.Token, request.Password));

        if (!result.Succeeded)
            return BadRequest(new { errors = result.Errors });

        return Ok(new { message = "Password reset successfully. You can now sign in with your new credentials." });
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
        // Retrieves the login information provided by the external OAuth2 provider.
        var info = await _signInManager.GetExternalLoginInfoAsync();
        if (info == null)
        {
            _logger.LogWarning("External login callback failed: no external login info found.");
            return BadRequest("External login failed.");
        }

        // Attempts to sign in the user if the external provider is already linked to an account.
        var signInResult = await _signInManager.ExternalLoginSignInAsync(
            info.LoginProvider, info.ProviderKey, isPersistent: false);

        IdentityUser? user;

        if (!signInResult.Succeeded)
        {
            // Extracts the email from external provider claims.
            var email = info.Principal.FindFirstValue(ClaimTypes.Email);
            if (string.IsNullOrEmpty(email))
            {
                return BadRequest("Email claim missing from external provider.");
            }

            // Checks if a local user with the same email already exists.
            user = await _userManager.FindByEmailAsync(email);

            if (user == null)
            {
                // Generates a display name from the provider's info or falls back to the email prefix.
                var displayName = info.Principal.FindFirstValue(ClaimTypes.Name) ?? email.Split('@')[0];

                // Ensures the generated UserName is unique within the database.
                if (await _userManager.FindByNameAsync(displayName) != null)
                {
                    displayName = $"{displayName}_{Guid.NewGuid().ToString().Substring(0, 4)}";
                }

                user = new IdentityUser { UserName = displayName, Email = email, EmailConfirmed = true };
                var createResult = await _userManager.CreateAsync(user);

                if (!createResult.Succeeded)
                {
                    _logger.LogError("Failed to auto-provision user for email {Email}.", email);
                    return BadRequest("Unable to create account from external login.");
                }

                // Assigns the default Reader role to the newly created external user.
                await _userManager.AddToRoleAsync(user, "Reader");
            }

            // Links the external login provider to the existing or newly created local account.
            await _userManager.AddLoginAsync(user, info);
        }
        else
        {
            // Retrieves the user associated with the successful external sign-in.
            user = await _userManager.FindByLoginAsync(info.LoginProvider, info.ProviderKey);
        }

        if (user == null) return BadRequest("Unable to resolve user after external login.");

        // Generates a JWT token for the authenticated user.
        var roles = await _userManager.GetRolesAsync(user);
        var token = _tokenService.GenerateToken(user, roles);

        // Redirects the client to the frontend callback route with the token and display name.
        var frontendUrl = $"/#/oauth/callback?token={Uri.EscapeDataString(token)}&username={Uri.EscapeDataString(user.UserName!)}";
        return Redirect(frontendUrl);
    }

    // --- Request Records ---

    /// <summary>The request body for the local login endpoint.</summary>
    public record LoginRequest(string Username, string Password);

    /// <summary>The request body for the registration endpoint.</summary>
    /// <remarks>UserName property is used as the public display name.</remarks>
    public record RegisterRequest(string UserName, string Email, string Password);

    /// <summary>The request body for the 2FA verification endpoint.</summary>
    public record VerifyTwoFactorRequest(string Username, string Code);

    /// <summary>The request body for the forgot password endpoint.</summary>
    public record ForgotPasswordRequest(string Email);

    /// <summary>The request body for the reset password endpoint.</summary>
    public record ResetPasswordRequest(string Email, string Token, string Password);
}

