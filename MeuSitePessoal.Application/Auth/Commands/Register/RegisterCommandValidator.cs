using FluentValidation;

namespace MeuSitePessoal.Application.Auth.Commands.Register;

/// <summary>
/// Validates the <see cref="RegisterCommand"/> to enforce email and password requirements
/// before the handler processes the registration request.
/// </summary>
public class RegisterCommandValidator : AbstractValidator<RegisterCommand>
{
    public RegisterCommandValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required.")
            .EmailAddress().WithMessage("A valid email address is required.");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password is required.")
            .MinimumLength(8).WithMessage("Password must be at least 8 characters.");
    }
}
