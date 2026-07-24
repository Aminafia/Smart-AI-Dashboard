using FluentValidation;

namespace Application.Features.Documents.Commands.UploadDocument;

public class UploadDocumentCommandValidator
    : AbstractValidator<UploadDocumentCommand>
{
    private const long MaxFileSize = 10 * 1024 * 1024;

    public UploadDocumentCommandValidator()
    {
        RuleFor(x => x.Document)
            .NotNull();

        RuleFor(x => x.Document.FileName)
            .NotEmpty();

        RuleFor(x => x.Document.Content)
            .NotNull();

        RuleFor(x => x.Document.Content.Length)
            .LessThanOrEqualTo(MaxFileSize)
            .WithMessage("Maximum file size is 10 MB.");
    }
}