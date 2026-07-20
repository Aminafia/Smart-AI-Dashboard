/*
AIJobStore Purpose:
- Responsible only for database operations
- No AI logic or controller logic.
- Only database access.
*/

using Core.Entities;

namespace Application.Interfaces;

public interface IAIJobStore
{
    Task AddJobAsync(AIJob job);

    Task<AIJob?> GetJobAsync(Guid id);

    Task<List<AIJob>> GetJobsAsync(
        int page,
        int pageSize);
    Task UpdateJobAsync(AIJob job);
}