using Application.Common.Models;
using Core.Entities;

namespace Application.Interfaces;

public interface IDocumentRepository
{
    Task AddAsync(Document document, CancellationToken cancellationToken);
    Task<Document?> GetByIdAsync(Guid id, Guid userId, CancellationToken cancellationToken = default);
    Task<PagedResponse<Document>> GetPagedAsync(int page, int pageSize, Guid userId, CancellationToken cancellationToken);
    Task DeleteAsync(Document document, CancellationToken cancellationToken);
}