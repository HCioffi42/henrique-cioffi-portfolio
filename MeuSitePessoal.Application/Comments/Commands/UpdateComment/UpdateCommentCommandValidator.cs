using FluentValidation;

namespace MeuSitePessoal.Application.Comments.Commands.UpdateComment;

/// <summary>
/// Validator for the UpdateCommentCommand.
/// </summary>
public class UpdateCommentCommandValidator : AbstractValidator<UpdateCommentCommand>
{
    public UpdateCommentCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Comment ID is required.");

        RuleFor(x => x.Content)
            .NotEmpty().WithMessage("Comment content cannot be empty.")
            .MaximumLength(2000).WithMessage("Comment content cannot exceed 2000 characters.");
    }
}
