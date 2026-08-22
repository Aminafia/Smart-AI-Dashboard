using FluentValidation;
using System.IO;
using Application.DTOs.Documents;

namespace Application.Features.Documents.Commands.UploadDocument;

public class UploadDocumentCommandValidator
    : AbstractValidator<UploadDocumentCommand>
{
    private const long MaxFileSize = 10 * 1024 * 1024;

    private static readonly Dictionary<string, string> AllowedFileTypes =
        new(StringComparer.OrdinalIgnoreCase)
        {
            [".pdf"] = "application/pdf",
            [".doc"] = "application/msword",
            [".docx"] = "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
            [".txt"] = "text/plain"
        };

    public UploadDocumentCommandValidator()
    {
        RuleFor(x => x.Document)
            .NotNull()
            .WithMessage("Document is required.");

        When(x => x.Document != null, () =>
        {
            RuleFor(x => x.Document.FileName)
                .NotEmpty()
                .WithMessage("File name is required.");

            RuleFor(x => x.Document.Content)
                .NotNull()
                .WithMessage("File content is required.");

            RuleFor(x => x.Document.Content.Length)
                .LessThanOrEqualTo(MaxFileSize)
                .WithMessage("Maximum file size is 10 MB.");

            RuleFor(x => x.Document.FileName)
                .Must(HaveAllowedExtension)
                .WithMessage("Only PDF, DOC, DOCX, and TXT files are supported.");

            RuleFor(x => x.Document)
                .Must(HaveMatchingContentType)
                .WithMessage("File extension and content type do not match.");
        });
    }

    private static bool HaveAllowedExtension(string fileName)
    {
        var extension = Path.GetExtension(fileName);

        return AllowedFileTypes.ContainsKey(extension);
    }

    private static bool HaveMatchingContentType(DocumentUpload document)
    {
        var extension = Path.GetExtension(document.FileName);

        return AllowedFileTypes.TryGetValue(
            extension,
            out var expectedContentType)
            && string.Equals(
                document.ContentType,
                expectedContentType,
                StringComparison.OrdinalIgnoreCase);
    }
}