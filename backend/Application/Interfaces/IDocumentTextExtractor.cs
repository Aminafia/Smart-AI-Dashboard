namespace Application.Interfaces;

public interface IDocumentTextExtractor
{
    Task<string> ExtractTextAsync(Stream content, string contentType, CancellationToken cancellationToken);
}