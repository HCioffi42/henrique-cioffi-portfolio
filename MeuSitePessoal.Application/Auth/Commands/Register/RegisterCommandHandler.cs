using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace MeuSitePessoal.Application.Auth.Commands.Register;

/// <summary>
/// Handles the <see cref="RegisterCommand"/> by creating a new IdentityUser
/// and assigning the default "Reader" role upon successful registration.
/// </summary>
public class RegisterCommandHandler : IRequestHandler<RegisterCommand, RegisterResult>
{
    private readonly UserManager<IdentityUser> _userManager;
    private readonly ILogger<RegisterCommandHandler> _logger;

    /// <summary>
    /// Initializes a new instance of <see cref="RegisterCommandHandler"/>.
    /// </summary>
    /// <param name="userManager">The ASP.NET Core Identity user manager.</param>
    /// <param name="logger">The logger instance.</param>
    public RegisterCommandHandler(UserManager<IdentityUser> userManager, ILogger<RegisterCommandHandler> logger)
    {
        _userManager = userManager;
        _logger = logger;
    }

    /// <summary>
    /// Processes the registration command by creating the user and assigning the Reader role.
    /// </summary>
    /// <param name="request">The registration command containing email and password.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A <see cref="RegisterResult"/> indicating success or failure with error messages.</returns>
    public async Task<RegisterResult> Handle(RegisterCommand request, CancellationToken cancellationToken)
    {
        var user = new IdentityUser
        {
            UserName = request.UserName,
            Email = request.Email,
            EmailConfirmed = true
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
        _logger.LogInformation("New Reader account registered for {Email}.", request.Email);

        return new RegisterResult(true, Enumerable.Empty<string>());
    }
}
