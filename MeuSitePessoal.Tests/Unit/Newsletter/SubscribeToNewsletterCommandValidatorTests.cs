using FluentValidation.TestHelper;
using MeuSitePessoal.Application.Newsletter.Commands.Subscribe;
using Xunit;

namespace MeuSitePessoal.Tests.Unit.Newsletter;

public class SubscribeToNewsletterCommandValidatorTests
{
    private readonly SubscribeToNewsletterCommandValidator _validator;

    public SubscribeToNewsletterCommandValidatorTests()
    {
        _validator = new SubscribeToNewsletterCommandValidator();
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void Should_HaveError_When_EmailIsEmpty(string email)
    {
        // Arrange
        var command = new SubscribeToNewsletterCommand(email);

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Email)
              .WithErrorMessage("Email address is required.");
    }

    [Theory]
    [InlineData("invalid-email")]
    [InlineData("@domain.com")]
    [InlineData("user")]
    public void Should_HaveError_When_EmailIsInvalid(string email)
    {
        // Arrange
        var command = new SubscribeToNewsletterCommand(email);

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Email)
              .WithErrorMessage("A valid email address is required.");
    }

    [Theory]
    [InlineData("test@test.com")]
    [InlineData("valid.user+alias@sub.domain.org")]
    public void Should_NotHaveError_When_EmailIsValid(string email)
    {
        // Arrange
        var command = new SubscribeToNewsletterCommand(email);

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Email);
    }
}
