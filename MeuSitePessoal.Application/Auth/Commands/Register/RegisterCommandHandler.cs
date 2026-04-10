using System.Text;
using MediatR;
using MeuSitePessoal.Application.Common.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace MeuSitePessoal.Application.Auth.Commands.Register;

/// <summary>
/// Handles the <see cref="RegisterCommand"/> by creating a new IdentityUser,
/// generating an email confirmation token, and sending a welcome email.
/// </summary>
public class RegisterCommandHandler : IRequestHandler<RegisterCommand, RegisterResult>
{
    private readonly UserManager<IdentityUser> _userManager;
    private readonly IEmailSender _emailSender;
    private readonly IConfiguration _configuration;
    private readonly ILogger<RegisterCommandHandler> _logger;

    /// <summary>
    /// Initializes a new instance of <see cref="RegisterCommandHandler"/>.
    /// </summary>
    /// <param name="userManager">The ASP.NET Core Identity user manager.</param>
    /// <param name="emailSender">The service for sending emails.</param>
    /// <param name="configuration">The application configuration.</param>
    /// <param name="logger">The logger instance.</param>
    public RegisterCommandHandler(
        UserManager<IdentityUser> userManager,
        IEmailSender emailSender,
        IConfiguration configuration,
        ILogger<RegisterCommandHandler> logger)
    {
        _userManager = userManager;
        _emailSender = emailSender;
        _configuration = configuration;
        _logger = logger;
    }

    /// <summary>
    /// Processes the registration command by creating the user, assigning roles, and sending a verification email.
    /// </summary>
    /// <param name="request">The registration command containing email and password.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A <see cref="RegisterResult"/> indicating success or failure with error messages.</returns>
    public async Task<RegisterResult> Handle(RegisterCommand request, CancellationToken cancellationToken)
    {
        var user = new IdentityUser
        {
            UserName = request.UserName,
            Email = request.Email
        };

        var result = await _userManager.CreateAsync(user, request.Password);

        if (!result.Succeeded)
        {
            var errors = result.Errors.Select(e => e.Description);
            var errorList = errors.ToList();
            _logger.LogWarning("Registration failed for {Email}: {Errors}", request.Email, string.Join(", ", errorList));
            return new RegisterResult(false, errorList);
        }

        await _userManager.AddToRoleAsync(user, "Reader");
        _logger.LogInformation("New Reader account created for {Email}. Proceeding with email confirmation flow.", request.Email);

        // Generate confirmation token and encode it for URL safety.
        var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
        var encodedCode = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));

        // Construct the callback URL using the base URL from configuration.
        var baseUrl = _configuration["ClientSettings:BaseUrl"] ?? "https://hcioffi.dev";
        var callbackUrl = $"{baseUrl}/verify-email?userId={user.Id}&token={encodedCode}";

        // Send the verification email.
        var subject = "Welcome to Meu Site Pessoal! Please confirm your email";
        var body = $@"
            <h1>Welcome, {user.UserName}!</h1>
            <p>Thank you for registering. Please confirm your email address by clicking the link below:</p>
            <p><a href='{callbackUrl}'>Confirm my email</a></p>
            <p>Or copy and paste this URL into your browser:</p>
            <p>{callbackUrl}</p>
        ";

        await _emailSender.SendEmailAsync(user.Email!, subject, body, cancellationToken);

        return new RegisterResult(true, Enumerable.Empty<string>());
    }
}

