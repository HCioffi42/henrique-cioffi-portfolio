using FluentValidation;

namespace MeuSitePessoal.Application.Newsletter.Commands.Subscribe;

/// <summary>
/// Validates the command to ensure the email address is correctly formatted.
/// </summary>
public class SubscribeToNewsletterCommandValidator : AbstractValidator<SubscribeToNewsletterCommand>
{
    /// <summary>
    /// Initializes a new instance of the validator with rules for the subscription command.
    /// </summary>
    public SubscribeToNewsletterCommandValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email address is required.")
            .EmailAddress().WithMessage("A valid email address is required.");
    }
}
