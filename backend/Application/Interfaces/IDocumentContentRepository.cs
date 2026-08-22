using Core.Entities;

namespace Application.Interfaces;

public interface IDocumentContentRepository
{
    Task<DocumentContent?> GetByDocumentIdAsync(Guid documentId, CancellationToken cancellationToken);
    Task AddAsync(DocumentContent content, CancellationToken cancellationToken);
    Task UpdateAsync(DocumentContent content, CancellationToken cancellationToken);
}