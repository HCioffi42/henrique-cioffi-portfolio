using FluentValidation.TestHelper;
using MeuSitePessoal.Application.Artigos.Commands.CreateArtigo;
using Xunit;

namespace MeuSitePessoal.Tests.Unit.Application.Artigos.Validators;

public class CreateArtigoCommandValidatorTests
{
    private readonly CreateArtigoCommandValidator _validator;

    public CreateArtigoCommandValidatorTests()
    {
        _validator = new CreateArtigoCommandValidator();
    }

    [Fact]
    public void Should_Not_Have_Error_When_Command_Is_Valid()
    {
        // Arrange
        var command = new CreateArtigoCommand(
            "Valid Title",
            "Valid Content",
            "Valid Summary",
            new List<string> { "tag1" }
        );

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Should_Have_Error_When_Titulo_Is_Empty(string? titulo)
    {
        // Arrange
        var command = new CreateArtigoCommand(titulo!, "Content", "Summary");

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Titulo)
            .WithErrorMessage("Title is required.");
    }

    [Fact]
    public void Should_Have_Error_When_Titulo_Exceeds_100_Characters()
    {
        // Arrange
        var longTitle = new string('a', 101);
        var command = new CreateArtigoCommand(longTitle, "Content", "Summary");

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Titulo)
            .WithErrorMessage("Title must not exceed 100 characters.");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void Should_Have_Error_When_Conteudo_Is_Empty(string? conteudo)
    {
        // Arrange
        var command = new CreateArtigoCommand("Title", conteudo!, "Summary");

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Conteudo)
            .WithErrorMessage("Content is required.");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Should_Have_Error_When_Resumo_Is_Empty(string? resumo)
    {
        // Arrange
        var command = new CreateArtigoCommand("Title", "Content", resumo!);

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Resumo)
            .WithErrorMessage("Summary is required.");
    }

    [Fact]
    public void Should_Have_Error_When_Resumo_Exceeds_500_Characters()
    {
        // Arrange
        var longSummary = new string('s', 501);
        var command = new CreateArtigoCommand("Title", "Content", longSummary);

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Resumo)
            .WithErrorMessage("Summary must not exceed 500 characters.");
    }

    [Fact]
    public void Should_Not_Have_Error_When_Tags_Is_Empty_But_Not_Null()
    {
        // Arrange
        var command = new CreateArtigoCommand("Title", "Content", "Summary", new List<string>());

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        // Tags are initialized in the record constructor if null, but we test the logic here.
        result.ShouldNotHaveValidationErrorFor(x => x.Tags);
    }
}
