using FluentValidation;

namespace MeuSitePessoal.Application.Artigos.Commands.CreateArtigo;

public class CreateArtigoCommandValidator : AbstractValidator<CreateArtigoCommand>
{
    public CreateArtigoCommandValidator()
    {
        RuleFor(v => v.Titulo)
            .NotEmpty().WithMessage("Title is required.")
            .MaximumLength(100).WithMessage("Title must not exceed 100 characters.");

        RuleFor(v => v.Conteudo)
            .NotEmpty().WithMessage("Content is required.");

        RuleFor(v => v.Resumo)
            .NotEmpty().WithMessage("Summary is required.")
            .MaximumLength(500).WithMessage("Summary must not exceed 500 characters.");
    }
}
