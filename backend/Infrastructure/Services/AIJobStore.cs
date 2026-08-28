using Application.Common.Models;
using Application.Interfaces;
using Core.Entities;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Services;

public class AIJobStore : IAIJobStore
{
    private readonly AppDbContext _dbContext;

    public AIJobStore(AppDbContext dbContext) => _dbContext = dbContext;

    public async Task AddJobAsync(AIJob job)
    {
        await _dbContext.AIJobs.AddAsync(job);
        await _dbContext.SaveChangesAsync();
    }

    public async Task<AIJob?> GetJobAsync(Guid id, Guid userId)
    {
        return await _dbContext.AIJobs.FirstOrDefaultAsync(x => x.Id == id && x.UserId == userId);
    }

    public async Task<PagedResponse<AIJob>> GetJobsAsync(int page, int pageSize, Guid userId)
    {
        var query = _dbContext.AIJobs.Where(x => x.UserId == userId).OrderByDescending(x => x.CreatedAt);
        var totalCount = await query.CountAsync();
        var jobs = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();

        return new PagedResponse<AIJob> { Items = jobs, Page = page, PageSize = pageSize, TotalCount = totalCount };
    }

    public async Task UpdateJobAsync(AIJob job)
    {
        _dbContext.AIJobs.Update(job);
        await _dbContext.SaveChangesAsync();
    }
}