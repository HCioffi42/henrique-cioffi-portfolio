using System.Text;
using MediatR;
using MeuSitePessoal.Application.Common.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MeuSitePessoal.Application.Common.Models.Email;

namespace MeuSitePessoal.Application.Auth.Commands.ForgotPassword;

/// <summary>
/// Handles the <see cref="ForgotPasswordCommand"/> by generating a password reset token
/// and sending it to the user's email if the account exists and is confirmed.
/// </summary>
public class ForgotPasswordCommandHandler : IRequestHandler<ForgotPasswordCommand, ForgotPasswordResult>
{
    private readonly UserManager<IdentityUser> _userManager;
    private readonly IEmailSender _emailSender;
    private readonly IEmailTemplateService _templateService;
    private readonly IConfiguration _configuration;
    private readonly ILogger<ForgotPasswordCommandHandler> _logger;

    private const string GenericSuccessMessage = "If your email is registered and confirmed, you will receive password reset instructions shortly.";

    /// <summary>
    /// Initializes a new instance of <see cref="ForgotPasswordCommandHandler"/>.
    /// </summary>
    public ForgotPasswordCommandHandler(
        UserManager<IdentityUser> userManager,
        IEmailSender emailSender,
        IEmailTemplateService templateService,
        IConfiguration configuration,
        ILogger<ForgotPasswordCommandHandler> logger)
    {
        _userManager = userManager;
        _emailSender = emailSender;
        _templateService = templateService;
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

        // Render and send the instructions via email using Razor templates.
        var subject = "Reset your password - hcioffi.dev";
        var body = await _templateService.RenderTemplateAsync("ResetPassword", new ResetPasswordViewModel
        { 
            UserName = user.UserName!,
            ResetLink = resetUrl
        });

        await _emailSender.SendEmailAsync(user.Email!, subject, body, cancellationToken);
        _logger.LogInformation("Password reset instructions sent to {Email}.", request.Email);

        return new ForgotPasswordResult(true, GenericSuccessMessage);
    }
}
