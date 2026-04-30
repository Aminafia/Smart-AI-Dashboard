using System.Collections.Concurrent;
using Core.Entities;
using Application.Interfaces;

namespace Infrastructure.Services;

public class AIJobStore : IAIJobStore
{
    private readonly ConcurrentDictionary<Guid, AIJob> _jobs = new();

    public void Add(AIJob job)
    {
        _jobs[job.Id] = job;
    }

    public AIJob? Get(Guid id)
    {
        _jobs.TryGetValue(id, out var job);
        return job;
    }

    public void Update(AIJob job)
    {
        _jobs[job.Id] = job;
    }
}