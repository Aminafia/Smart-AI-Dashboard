/*
AIQueue Purpose: -
- Temporary in-memory queue for AIJobs
- Handler will enqueues jobs 
- AIWorker dequeues and processes jobs in background  
*/

using System.Collections.Concurrent;
using Core.Entities;
using Application.Interfaces;

namespace Infrastructure.Services;

public class AIQueue : IAIQueue
{
    private readonly ConcurrentQueue<AIJob> _queue = new();

    public void Enqueue(AIJob job)
    {
        _queue.Enqueue(job);
    }

    public AIJob? Dequeue()
    {
        _queue.TryDequeue(out var job);
        return job;
    }
}