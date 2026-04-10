using System.Text;
using MediatR;
using MeuSitePessoal.Application.Common.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace MeuSitePessoal.Application.Auth.Commands.ForgotPassword;

/// <summary>
/// Handles the <see cref="ForgotPasswordCommand"/> by generating a password reset token
/// and sending it to the user's email if the account exists and is confirmed.
/// </summary>
public class ForgotPasswordCommandHandler : IRequestHandler<ForgotPasswordCommand, ForgotPasswordResult>
{
    private readonly UserManager<IdentityUser> _userManager;
    private readonly IEmailSender _emailSender;
    private readonly IConfiguration _configuration;
    private readonly ILogger<ForgotPasswordCommandHandler> _logger;

    private const string GenericSuccessMessage = "If your email is registered and confirmed, you will receive password reset instructions shortly.";

    /// <summary>
    /// Initializes a new instance of <see cref="ForgotPasswordCommandHandler"/>.
    /// </summary>
    /// <param name="userManager">The Identity user manager.</param>
    /// <param name="emailSender">The service for sending emails.</param>
    /// <param name="configuration">The application configuration.</param>
    /// <param name="logger">The logger instance.</param>
    public ForgotPasswordCommandHandler(
        UserManager<IdentityUser> userManager,
        IEmailSender emailSender,
        IConfiguration configuration,
        ILogger<ForgotPasswordCommandHandler> logger)
    {
        _userManager = userManager;
        _emailSender = emailSender;
        _configuration = configuration;
        _logger = logger;
    }

    /// <summary>
    /// Processes the forgot password request.
    /// Returns a generic success message regardless of user existence to prevent account enumeration.
    /// </summary>
    /// <param name="request">The command containing the user email.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A <see cref="ForgotPasswordResult"/> with a generic message.</returns>
    public async Task<ForgotPasswordResult> Handle(ForgotPasswordCommand request, CancellationToken cancellationToken)
    {
        var user = await _userManager.FindByEmailAsync(request.Email);

        // Security check: Only proceed if the user exists and has confirmed their email.
        if (user == null || !(await _userManager.IsEmailConfirmedAsync(user)))
        {
            _logger.LogInformation("Forgot password attempt for non-existent or unconfirmed email: {Email}. Returning generic success.", request.Email);
            return new ForgotPasswordResult(true, GenericSuccessMessage);
        }

        // Generate the password reset token and encode it for URL transport.
        var token = await _userManager.GeneratePasswordResetTokenAsync(user);
        var encodedToken = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token));

        // Construct the reset URL using the dynamic BaseUrl from configuration.
        var baseUrl = _configuration["ClientSettings:BaseUrl"] ?? "https://hcioffi.dev";
        var resetUrl = $"{baseUrl}/reset-password?email={Uri.EscapeDataString(user.Email!)}&token={encodedToken}";

        // Send the instructions via email.
        var subject = "Reset your password - Meu Site Pessoal";
        var body = $@"
            <h1>Password Reset Request</h1>
            <p>Hello, {user.UserName}!</p>
            <p>We received a request to reset your password. If you didn't do this, just ignore this email.</p>
            <p>To set a new password, click the link below:</p>
            <p><a href='{resetUrl}'>Reset Password</a></p>
            <p>Or copy and paste this link into your browser:</p>
            <p>{resetUrl}</p>
        ";

        await _emailSender.SendEmailAsync(user.Email!, subject, body, cancellationToken);
        _logger.LogInformation("Password reset instructions sent to {Email}.", request.Email);

        return new ForgotPasswordResult(true, GenericSuccessMessage);
    }
}
