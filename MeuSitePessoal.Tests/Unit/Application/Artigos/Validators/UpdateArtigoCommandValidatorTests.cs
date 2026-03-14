using FluentValidation.TestHelper;
using MeuSitePessoal.Application.Artigos.Commands.UpdateArtigo;
using Xunit;

namespace MeuSitePessoal.Tests.Unit.Application.Artigos.Validators;

public class UpdateArtigoCommandValidatorTests
{
    private readonly UpdateArtigoCommandValidator _validator;

    public UpdateArtigoCommandValidatorTests()
    {
        _validator = new UpdateArtigoCommandValidator();
    }

    [Fact]
    public void Should_Not_Have_Error_When_Command_Is_Valid()
    {
        // Arrange
        var command = new UpdateArtigoCommand(
            Guid.NewGuid(),
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

    [Fact]
    public void Should_Have_Error_When_Id_Is_Empty()
    {
        // Arrange
        var command = new UpdateArtigoCommand(Guid.Empty, "Title", "Content", "Summary");

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Id)
            .WithErrorMessage("Id is required.");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Should_Have_Error_When_Titulo_Is_Empty(string? titulo)
    {
        // Arrange
        var command = new UpdateArtigoCommand(Guid.NewGuid(), titulo!, "Content", "Summary");

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
        var command = new UpdateArtigoCommand(Guid.NewGuid(), longTitle, "Content", "Summary");

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
        var command = new UpdateArtigoCommand(Guid.NewGuid(), "Title", conteudo!, "Summary");

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
        var command = new UpdateArtigoCommand(Guid.NewGuid(), "Title", "Content", resumo!);

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
        var command = new UpdateArtigoCommand(Guid.NewGuid(), "Title", "Content", longSummary);

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Resumo)
            .WithErrorMessage("Summary must not exceed 500 characters.");
    }
}
