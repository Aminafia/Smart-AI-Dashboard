using Application.Interfaces;
using UglyToad.PdfPig;
using UglyToad.PdfPig.DocumentLayoutAnalysis.TextExtractor;

namespace Infrastructure.DocumentProcessing;

public class PdfTextExtractor : IDocumentTextExtractor
{
    public Task<string> ExtractTextAsync(
        Stream content,
        string contentType,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        using var document = PdfDocument.Open(content);

        var pages = document
            .GetPages()
            .Select(page =>
            {
                cancellationToken.ThrowIfCancellationRequested();

                return ContentOrderTextExtractor.GetText(page);
            });

        var text = string.Join(
            Environment.NewLine,
            pages);

        cancellationToken.ThrowIfCancellationRequested();

        return Task.FromResult(text);
    }
}