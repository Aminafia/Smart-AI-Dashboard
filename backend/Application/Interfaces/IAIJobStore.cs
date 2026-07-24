/*
AIJobStore Purpose:
- Responsible only for database operations
- No AI logic or controller logic.
- Only database access.
*/

using Core.Entities;
using Application.Common.Models;

namespace Application.Interfaces;

public interface IAIJobStore
{
    Task AddJobAsync(AIJob job);

    Task<AIJob?> GetJobAsync(Guid id);

    Task<PagedResponse<AIJob>> GetJobsAsync(
        int page,
        int pageSize);
    Task UpdateJobAsync(AIJob job);
}