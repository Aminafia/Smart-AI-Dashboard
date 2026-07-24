using Application.DTOs.Documents;

namespace Application.Interfaces;

public interface IDocumentStorage
{
    Task<string> SaveAsync(DocumentUpload document);
    Task DeleteAsync(string storagePath);
    Task<Stream> OpenReadAsync(string storagePath);
}