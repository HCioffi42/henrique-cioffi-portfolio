using FluentValidation.TestHelper;
using MeuSitePessoal.Application.Comments.Commands.CreateComment;
using Xunit;

namespace MeuSitePessoal.Tests.Unit.Comments;

public class CreateCommentCommandValidatorTests
{
    private readonly CreateCommentCommandValidator _validator;

    public CreateCommentCommandValidatorTests()
    {
        _validator = new CreateCommentCommandValidator();
    }

    [Fact]
    public void Should_Have_Error_When_ArticleId_Is_Empty()
    {
        var command = new CreateCommentCommand(Guid.Empty, "Content");
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.ArticleId);
    }

    [Fact]
    public void Should_Have_Error_When_Content_Is_Empty()
    {
        var command = new CreateCommentCommand(Guid.NewGuid(), "");
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.Content);
    }

    [Fact]
    public void Should_Have_Error_When_Content_Exceeds_MaxLength()
    {
        var command = new CreateCommentCommand(Guid.NewGuid(), new string('a', 2001));
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.Content);
    }

    [Fact]
    public void Should_Not_Have_Error_When_Command_Is_Valid()
    {
        var command = new CreateCommentCommand(Guid.NewGuid(), "Valid Content");
        var result = _validator.TestValidate(command);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Should_Not_Have_Error_When_Reply_Is_Valid()
    {
        var command = new CreateCommentCommand(Guid.NewGuid(), "Reply", Guid.NewGuid());
        var result = _validator.TestValidate(command);
        result.ShouldNotHaveAnyValidationErrors();
    }
}
