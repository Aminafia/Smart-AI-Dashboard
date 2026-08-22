using Application.DTOs.Documents;

namespace Application.Interfaces;

public interface IDocumentStorage
{
    Task<string> SaveAsync(DocumentUpload document, CancellationToken cancellationToken);
    Task DeleteAsync(string storagePath, CancellationToken cancellationToken);
    Task<Stream> OpenReadAsync(string storagePath, CancellationToken cancellationToken);
}