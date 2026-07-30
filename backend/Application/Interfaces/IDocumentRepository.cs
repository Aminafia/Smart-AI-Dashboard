using Application.Common.Models;
using Core.Entities;

namespace Application.Interfaces;

public interface IDocumentRepository
{
    Task AddAsync(Document document);

    Task<Document?> GetByIdAsync(Guid id);

    Task<PagedResponse<Document>> GetPagedAsync(int page, int pageSize);

    Task DeleteAsync(Document document);
}