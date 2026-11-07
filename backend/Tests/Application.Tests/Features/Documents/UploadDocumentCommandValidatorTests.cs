using Application.DTOs.Documents;
using Application.Features.Documents.Commands.UploadDocument;
using FluentValidation.TestHelper;

namespace Application.Tests.Features.Documents;

public class UploadDocumentCommandValidatorTests
{
    private const int MaxFileSize = 10 * 1024 * 1024;
    private readonly UploadDocumentCommandValidator _validator = new();

    private static UploadDocumentCommand CreateCommand(
        string fileName = "report.pdf",
        string contentType = "application/pdf",
        Stream? content = null) =>
        new()
        {
            Document = new DocumentUpload
            {
                FileName = fileName,
                ContentType = contentType,
                Content = content ?? new MemoryStream([1, 2, 3])
            }
        };

    [Fact]
    public void Validate_NullDocument_HasRequiredError()
    {
        var result = _validator.TestValidate(new UploadDocumentCommand { Document = null! });
        result.ShouldHaveValidationErrorFor(x => x.Document);
    }

    [Fact]
    public void Validate_NullContent_HasContentRequiredErrorWithoutThrowing()
    {
        var command = CreateCommand();
        command.Document.Content = null!;

        var result = Record.Exception(() => _validator.TestValidate(command));

        Assert.Null(result);
        _validator.TestValidate(command).ShouldHaveValidationErrorFor("Document.Content.Length");
    }

    [Theory]
    [InlineData("report.pdf", "application/pdf")]
    [InlineData("report.DOC", "application/msword")]
    [InlineData("report.docx", "application/vnd.openxmlformats-officedocument.wordprocessingml.document")]
    [InlineData("notes.txt", "text/plain")]
    public void Validate_AllowedExtensionAndMatchingContentType_IsAccepted(string fileName, string contentType)
    {
        var result = _validator.TestValidate(CreateCommand(fileName, contentType));
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData("image.png")]
    [InlineData("report.exe")]
    [InlineData("report")]
    public void Validate_UnsupportedExtension_HasError(string fileName)
    {
        var result = _validator.TestValidate(CreateCommand(fileName));
        result.ShouldHaveValidationErrorFor("Document.FileName");
    }

    [Fact]
    public void Validate_MismatchedContentType_HasError()
    {
        var result = _validator.TestValidate(CreateCommand("report.pdf", "text/plain"));
        result.ShouldHaveValidationErrorFor(x => x.Document);
    }

    [Fact]
    public void Validate_FileExactlyTenMegabytes_IsAccepted()
    {
        var result = _validator.TestValidate(CreateCommand(content: new MemoryStream(new byte[MaxFileSize])));
        result.ShouldNotHaveValidationErrorFor("Document.Content");
    }

    [Fact]
    public void Validate_FileLargerThanTenMegabytes_HasError()
    {
        var result = _validator.TestValidate(CreateCommand(content: new MemoryStream(new byte[MaxFileSize + 1])));
        result.ShouldHaveValidationErrorFor("Document.Content");
    }
}
