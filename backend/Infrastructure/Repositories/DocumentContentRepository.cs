using Application.Interfaces;
using Core.Entities;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public class DocumentContentRepository : IDocumentContentRepository
{
    private readonly AppDbContext _dbContext;

    public DocumentContentRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<DocumentContent?> GetByDocumentIdAsync(
        Guid documentId,
        CancellationToken cancellationToken)
    {
        return await _dbContext.DocumentContents
            .FirstOrDefaultAsync(
                x => x.DocumentId == documentId,
                cancellationToken);
    }

    public async Task AddAsync(
        DocumentContent content,
        CancellationToken cancellationToken)
    {
        _dbContext.DocumentContents.Add(content);

        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(
        DocumentContent content,
        CancellationToken cancellationToken)
    {
        _dbContext.DocumentContents.Update(content);

        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}