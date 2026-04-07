using FluentValidation;

namespace MeuSitePessoal.Application.Comments.Commands.CreateComment;

/// <summary>
/// Validator for the CreateCommentCommand.
/// Ensures that comment content is provided and within length constraints.
/// </summary>
public class CreateCommentCommandValidator : AbstractValidator<CreateCommentCommand>
{
    /// <summary>
    /// Initializes the validation rules.
    /// </summary>
    public CreateCommentCommandValidator()
    {
        RuleFor(x => x.ArticleId)
            .NotEmpty().WithMessage("The article ID is required.");

        RuleFor(x => x.Content)
            .NotEmpty().WithMessage("Comment content cannot be empty.")
            .MaximumLength(2000).WithMessage("Comment content cannot exceed 2000 characters.");

        // ParentCommentId is optional, so we only validate it if it's provided.
        RuleFor(x => x.ParentCommentId)
            .NotEqual(Guid.Empty).When(x => x.ParentCommentId.HasValue)
            .WithMessage("Parent comment ID cannot be an empty GUID.");
    }
}
