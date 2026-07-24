using Application.Common.Models;
using Application.Interfaces;
using Core.Entities;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Services;

public class DocumentStore : IDocumentStore
{
    private readonly AppDbContext _dbContext;

    public DocumentStore(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task AddAsync(Document document)
    {
        _dbContext.Documents.Add(document);
        await _dbContext.SaveChangesAsync();
    }

    public async Task<Document?> GetByIdAsync(Guid id)
    {
        return await _dbContext.Documents
            .FirstOrDefaultAsync(x => x.Id == id);
    }

    public async Task<PagedResponse<Document>> GetDocumentsAsync(int page, int pageSize)
    {
        var query = _dbContext.Documents
            .OrderByDescending(x => x.UploadedAt);

        var totalCount = await query.CountAsync();

        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return new PagedResponse<Document>
        {
            Items = items,
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount
        };
    }

    public async Task DeleteAsync(Guid id)
    {
        var document = await GetByIdAsync(id);

        if (document == null)
            return;

        _dbContext.Documents.Remove(document);

        await _dbContext.SaveChangesAsync();
    }
}