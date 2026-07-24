using Application.Common.Models;
using Core.Entities;

namespace Application.Interfaces;

public interface IDocumentStore
{
    Task AddAsync(Document document);

    Task<Document?> GetByIdAsync(Guid id);

    Task<PagedResponse<Document>> GetDocumentsAsync(int page, int pageSize);

    Task DeleteAsync(Guid id);
}