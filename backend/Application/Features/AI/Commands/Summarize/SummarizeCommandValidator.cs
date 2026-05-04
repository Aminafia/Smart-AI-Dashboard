using FluentValidation;

namespace Application.Features.AI.Commands.Summarize;

public class SummarizeCommandValidator : AbstractValidator<SummarizeCommand>
{
    public SummarizeCommandValidator()
    {
        RuleFor(x => x.Text)
            .NotEmpty().WithMessage("Text cannot be empty")
            .MinimumLength(5).WithMessage("Text must be at least 5 characters");
    }
}