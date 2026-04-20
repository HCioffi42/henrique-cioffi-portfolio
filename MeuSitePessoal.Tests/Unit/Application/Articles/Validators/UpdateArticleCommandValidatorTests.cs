using FluentValidation.TestHelper;
using MeuSitePessoal.Application.Articles.Commands.UpdateArticle;
using MeuSitePessoal.Domain;
using MeuSitePessoal.Domain.Entities;
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
            "Valid Title EN",
            "Valid Title PT",
            "Valid Content EN",
            "Valid Content PT",
            "Valid Summary EN",
            "Valid Summary PT",
            ArticleCategory.Technology,
            new List<string> { "tag1" }
        );

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public async Task Should_Have_Error_When_Id_Is_Empty()
    {
        // Arrange
        var command = new UpdateArticleCommand(Guid.Empty, "Title", "Title", "Content", "Content", "Summary", "Summary", ArticleCategory.Technology);

        // Act
        var result = await _validator.TestValidateAsync(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Id)
            .WithErrorMessage("Id is required.");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task Should_Have_Error_When_English_Title_Is_Empty(string? title)
    {
        // Arrange
        var command = new UpdateArticleCommand(Guid.NewGuid(), title!, "Title PT", "Content EN", "Content PT", "Summary EN", "Summary PT", ArticleCategory.Technology);

        // Act
        var result = await _validator.TestValidateAsync(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.TitleEn)
            .WithErrorMessage("English Title is required.");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task Should_Have_Error_When_Portuguese_Title_Is_Empty(string? title)
    {
        // Arrange
        var command = new UpdateArticleCommand(Guid.NewGuid(), "Title EN", title!, "Content EN", "Content PT", "Summary EN", "Summary PT", ArticleCategory.Technology);

        // Act
        var result = await _validator.TestValidateAsync(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.TitlePt)
            .WithErrorMessage("Portuguese Title is required.");
    }

    [Fact]
    public async Task Should_Have_Error_When_English_Title_Exceeds_100_Characters()
    {
        // Arrange
        var longTitle = new string('a', 101);
        var command = new UpdateArticleCommand(Guid.NewGuid(), longTitle, "Title PT", "Content EN", "Content PT", "Summary EN", "Summary PT", ArticleCategory.Technology);

        // Act
        var result = await _validator.TestValidateAsync(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.TitleEn)
            .WithErrorMessage("English Title must not exceed 100 characters.");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public async Task Should_Have_Error_When_English_Content_Is_Empty(string? content)
    {
        // Arrange
        var command = new UpdateArticleCommand(Guid.NewGuid(), "Title EN", "Title PT", content!, "Content PT", "Summary EN", "Summary PT", ArticleCategory.Technology);

        // Act
        var result = await _validator.TestValidateAsync(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.ContentEn)
            .WithErrorMessage("English Content is required.");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task Should_Have_Error_When_English_Summary_Is_Empty(string? summary)
    {
        // Arrange
        var command = new UpdateArticleCommand(Guid.NewGuid(), "Title EN", "Title PT", "Content EN", "Content PT", summary!, "Summary PT", ArticleCategory.Technology);

        // Act
        var result = await _validator.TestValidateAsync(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.SummaryEn)
            .WithErrorMessage("English Summary is required.");
    }

    [Fact]
    public async Task Should_Have_Error_When_English_Summary_Exceeds_500_Characters()
    {
        // Arrange
        var longSummary = new string('s', 501);
        var command = new UpdateArticleCommand(Guid.NewGuid(), "Title EN", "Title PT", "Content EN", "Content PT", longSummary, "Summary PT", ArticleCategory.Technology);

        // Act
        var result = await _validator.TestValidateAsync(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.SummaryEn)
            .WithErrorMessage("English Summary must not exceed 500 characters.");
    }

    [Fact]
    public async Task Should_Have_Error_When_Category_Is_Invalid()
    {
        // Arrange
        var command = new UpdateArticleCommand(Guid.NewGuid(), "Title EN", "Title PT", "Content EN", "Content PT", "Summary EN", "Summary PT", (ArticleCategory)999);

        // Act
        var result = await _validator.TestValidateAsync(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Category)
            .WithErrorMessage("A valid category is required.");
    }
}
