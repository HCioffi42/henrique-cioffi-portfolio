using FluentValidation.TestHelper;
using MeuSitePessoal.Application.Articles.Commands.CreateArticle;
using MeuSitePessoal.Domain;
using MeuSitePessoal.Domain.Entities;
using Xunit;

namespace MeuSitePessoal.Tests.Unit.Application.Articles.Validators;

public class CreateArticleCommandValidatorTests
{
    private readonly CreateArticleCommandValidator _validator;

    public CreateArticleCommandValidatorTests()
    {
        _validator = new CreateArticleCommandValidator();
    }

    [Fact]
    public void Should_Not_Have_Error_When_Command_Is_Valid()
    {
        // Arrange
        var command = new CreateArticleCommand(
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

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Should_Have_Error_When_Title_Is_Empty(string? title)
    {
        // Arrange
        var command = new CreateArticleCommand(title!, "Content", "Summary", ArticleCategory.Technology);

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Title)
            .WithErrorMessage("Title is required.");
    }

    [Fact]
    public void Should_Have_Error_When_Title_Exceeds_100_Characters()
    {
        // Arrange
        var longTitle = new string('a', 101);
        var command = new CreateArticleCommand(longTitle, "Content", "Summary", ArticleCategory.Technology);

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Title)
            .WithErrorMessage("Title must not exceed 100 characters.");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void Should_Have_Error_When_Content_Is_Empty(string? content)
    {
        // Arrange
        var command = new CreateArticleCommand("Title", content!, "Summary", ArticleCategory.Technology);

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
    public void Should_Have_Error_When_Summary_Is_Empty(string? rummary)
    {
        // Arrange
        var command = new CreateArticleCommand("Title", "Content", rummary!, ArticleCategory.Technology);

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Summary)
            .WithErrorMessage("Summary is required.");
    }

    [Fact]
    public void Should_Have_Error_When_Summary_Exceeds_500_Characters()
    {
        // Arrange
        var longSummary = new string('s', 501);
        var command = new CreateArticleCommand("Title", "Content", longSummary, ArticleCategory.Technology);

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Summary)
            .WithErrorMessage("Summary must not exceed 500 characters.");
    }

    [Fact]
    public void Should_Not_Have_Error_When_Tags_Is_Empty_But_Not_Null()
    {
        // Arrange
        var command = new CreateArticleCommand("Title", "Content", "Summary", ArticleCategory.Technology, new List<string>());

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        // Tags are initialized in the record constructor if null, but we test the logic here.
        result.ShouldNotHaveValidationErrorFor(x => x.Tags);
    }

    [Fact]
    public void Should_Have_Error_When_Category_Is_Invalid()
    {
        // Arrange
        var command = new CreateArticleCommand("Title", "Content", "Summary", (ArticleCategory)999);

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Category)
            .WithErrorMessage("A valid category is required.");
    }
}
