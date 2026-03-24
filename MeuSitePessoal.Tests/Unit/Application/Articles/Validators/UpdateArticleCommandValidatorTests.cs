using FluentValidation.TestHelper;
using MeuSitePessoal.Application.Articles.Commands.UpdateArticle;
using MeuSitePessoal.Domain;
using Xunit;

namespace MeuSitePessoal.Tests.Unit.Application.Articles.Validators;

public class UpdateArticleCommandValidatorTests
{
    private readonly UpdateArticleCommandValidator _validator;

    public UpdateArticleCommandValidatorTests()
    {
        _validator = new UpdateArticleCommandValidator();
    }

    [Fact]
    public void Should_Not_Have_Error_When_Command_Is_Valid()
    {
        // Arrange
        var command = new UpdateArticleCommand(
            Guid.NewGuid(),
            "Valid Title",
            "Valid Content",
            "Valid Summary",
            ArticleCategory.Technology,
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
        var command = new UpdateArticleCommand(Guid.Empty, "Title", "Content", "Summary", ArticleCategory.Technology);

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
        var command = new UpdateArticleCommand(Guid.NewGuid(), titulo!, "Content", "Summary", ArticleCategory.Technology);

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Title)
            .WithErrorMessage("Title is required.");
    }

    [Fact]
    public void Should_Have_Error_When_Titulo_Exceeds_100_Characters()
    {
        // Arrange
        var longTitle = new string('a', 101);
        var command = new UpdateArticleCommand(Guid.NewGuid(), longTitle, "Content", "Summary", ArticleCategory.Technology);

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Title)
            .WithErrorMessage("Title must not exceed 100 characters.");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void Should_Have_Error_When_Conteudo_Is_Empty(string? conteudo)
    {
        // Arrange
        var command = new UpdateArticleCommand(Guid.NewGuid(), "Title", conteudo!, "Summary", ArticleCategory.Technology);

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Content)
            .WithErrorMessage("Content is required.");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Should_Have_Error_When_Resumo_Is_Empty(string? resumo)
    {
        // Arrange
        var command = new UpdateArticleCommand(Guid.NewGuid(), "Title", "Content", resumo!, ArticleCategory.Technology);

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Summary)
            .WithErrorMessage("Summary is required.");
    }

    [Fact]
    public void Should_Have_Error_When_Resumo_Exceeds_500_Characters()
    {
        // Arrange
        var longSummary = new string('s', 501);
        var command = new UpdateArticleCommand(Guid.NewGuid(), "Title", "Content", longSummary, ArticleCategory.Technology);

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Summary)
            .WithErrorMessage("Summary must not exceed 500 characters.");
    }

    [Fact]
    public void Should_Have_Error_When_Category_Is_Invalid()
    {
        // Arrange
        var command = new UpdateArticleCommand(Guid.NewGuid(), "Title", "Content", "Summary", (ArticleCategory)999);

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Category)
            .WithErrorMessage("A valid category is required.");
    }
}
