using FluentValidation;

namespace MeuSitePessoal.Application.Articles.Commands.UpdateArticle;

public class UpdateArticleCommandValidator : AbstractValidator<UpdateArticleCommand>
{
    public UpdateArticleCommandValidator()
    {
        RuleFor(v => v.Id)
            .NotEmpty().WithMessage("Id is required.");

        // English validation
        RuleFor(v => v.TitleEn)
            .NotEmpty().WithMessage("English Title is required.")
            .MaximumLength(100).WithMessage("English Title must not exceed 100 characters.");

        RuleFor(v => v.ContentEn)
            .NotEmpty().WithMessage("English Content is required.");

        RuleFor(v => v.SummaryEn)
            .NotEmpty().WithMessage("English Summary is required.")
            .MaximumLength(500).WithMessage("English Summary must not exceed 500 characters.");

        // Portuguese validation
        RuleFor(v => v.TitlePt)
            .NotEmpty().WithMessage("Portuguese Title is required.")
            .MaximumLength(100).WithMessage("Portuguese Title must not exceed 100 characters.");

        RuleFor(v => v.ContentPt)
            .NotEmpty().WithMessage("Portuguese Content is required.");

        RuleFor(v => v.SummaryPt)
            .NotEmpty().WithMessage("Portuguese Summary is required.")
            .MaximumLength(500).WithMessage("Portuguese Summary must not exceed 500 characters.");

        RuleFor(v => v.Category)
            .IsInEnum().WithMessage("A valid category is required.");
    }
}
