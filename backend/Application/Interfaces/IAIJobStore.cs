using Application.Common.Models;
using Application.Features.AI.Queries.GetAIStats;
using Core.Entities;

namespace Application.Interfaces;

public interface IAIJobStore
{
    Task AddJobAsync(AIJob job);
    Task<AIJob?> GetJobAsync(Guid id, Guid userId);
    Task<PagedResponse<AIJob>> GetJobsAsync(int page, int pageSize, Guid userId);
    Task UpdateJobAsync(AIJob job);
    Task<AIStatsResponse> GetStatsAsync(Guid userId);
}