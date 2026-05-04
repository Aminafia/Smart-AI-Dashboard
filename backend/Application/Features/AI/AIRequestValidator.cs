using FluentValidation;
using Application.Features.AI.Commands.GenerateAI;

namespace Application.Features.AI.Commands.GenerateAI;

public class GenerateAICommandValidator : AbstractValidator<GenerateAICommand>
{
    public GenerateAICommandValidator()
    {
        RuleFor(x => x.Prompt)
            .NotEmpty()
            .WithMessage("Prompt cannot be empty")

            .MinimumLength(5)
            .WithMessage("Prompt must be at least 5 characters")

            .MaximumLength(1000)
            .WithMessage("Prompt cannot exceed 1000 characters");
    }
}