/*
AIJobStore Purpose:
- Responsible only for AIJob database operations.
- Provides CRUD operations for AIJob entities.
- Contains no AI processing or business logic.
*/

using Application.Common.Models;
using Application.Interfaces;
using Core.Entities;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Services;

public class AIJobStore : IAIJobStore
{
    private readonly AppDbContext _dbContext;

    public AIJobStore(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task AddJobAsync(AIJob job)
    {
        await _dbContext.AIJobs.AddAsync(job);

        await _dbContext.SaveChangesAsync();
    }

    public async Task<AIJob?> GetJobAsync(Guid id)
    {
        return await _dbContext.AIJobs
            .FirstOrDefaultAsync(x => x.Id == id);
    }

    public async Task<PagedResponse<AIJob>> GetJobsAsync(int page, int pageSize)
    {
        var totalCount = await _dbContext.AIJobs.CountAsync();

        var jobs = await _dbContext.AIJobs
            .OrderByDescending(x => x.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return new PagedResponse<AIJob>
        {
            Items = jobs,
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount
        };
    }

    public async Task UpdateJobAsync(AIJob job)
    {
        _dbContext.AIJobs.Update(job);

        await _dbContext.SaveChangesAsync();
    }
}