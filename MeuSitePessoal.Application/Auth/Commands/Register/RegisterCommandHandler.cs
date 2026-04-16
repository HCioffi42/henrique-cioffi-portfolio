using System.Text;
using MediatR;
using MeuSitePessoal.Application.Common.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MeuSitePessoal.Application.Common.Models.Email;
using MeuSitePessoal.Domain.Interfaces;

namespace MeuSitePessoal.Application.Auth.Commands.Register;

/// <summary>
/// Handles the <see cref="RegisterCommand"/> by creating a new IdentityUser,
/// generating an email confirmation token, and sending a welcome email.
/// </summary>
public class RegisterCommandHandler : IRequestHandler<RegisterCommand, RegisterResult>
{
    private readonly UserManager<IdentityUser> _userManager;
    private readonly IEmailSender _emailSender;
    private readonly IEmailTemplateService _templateService;
    private readonly IConfiguration _configuration;
    private readonly ILogger<RegisterCommandHandler> _logger;
    private readonly ISubscriberRepository _subscriberRepository;

    /// <summary>
    /// Initializes a new instance of <see cref="RegisterCommandHandler"/>.
    /// </summary>
    public RegisterCommandHandler(
        UserManager<IdentityUser> userManager,
        IEmailSender emailSender,
        IEmailTemplateService templateService,
        IConfiguration configuration,
        ILogger<RegisterCommandHandler> logger,
        ISubscriberRepository subscriberRepository)
    {
        _userManager = userManager;
        _emailSender = emailSender;
        _templateService = templateService;
        _configuration = configuration;
        _logger = logger;
        _subscriberRepository = subscriberRepository;
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

        // HC: Seamlessly add the user to the newsletter subscribers list as well.
        // They will be considered "Verified" once they confirm their account email.
        var subscriber = await _subscriberRepository.GetByEmailAsync(request.Email);
        if (subscriber == null)
        {
            // Constructor handles ID, Tokens, and Dates.
            await _subscriberRepository.AddAsync(new Domain.Entities.Subscriber(request.Email));
        }

        // Generate confirmation token and encode it for URL safety.
        var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
        var encodedCode = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));

        // Construct the callback URL using the base URL from configuration.
        var baseUrl = _configuration["ClientSettings:BaseUrl"] ?? "https://hcioffi.dev";
        var callbackUrl = $"{baseUrl}/verify-email?userId={user.Id}&token={encodedCode}";

        // Render and send the verification email using Razor templates.
        var subject = "Welcome to hcioffi.dev | Verify your email";
        var body = await _templateService.RenderTemplateAsync("ConfirmAccount", new ConfirmAccountViewModel
        { 
            UserName = user.UserName!, 
            ConfirmLink = callbackUrl 
        });

        await _emailSender.SendEmailAsync(user.Email!, subject, body, cancellationToken);

        return new RegisterResult(true, Enumerable.Empty<string>());
    }
}
