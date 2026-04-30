using Application.DTOs.AI;
using Application.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Infrastructure.BackgroundServices;

public class AIWorker : BackgroundService
{
    private readonly IAIQueue _queue;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IAIJobStore _jobStore;
    private readonly ILogger<AIWorker> _logger;

    public AIWorker(
        IAIQueue queue,
        IServiceScopeFactory scopeFactory,
        IAIJobStore jobStore,
        ILogger<AIWorker> logger)
    {
        _queue = queue;
        _scopeFactory = scopeFactory;
        _jobStore = jobStore;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            var job = _queue.Dequeue();

            if (job is null)
            {
                await Task.Delay(500, stoppingToken);
                continue;
            }

            try
            {
                job.Status = "Processing";
                _jobStore.Update(job);

                using var scope = _scopeFactory.CreateScope();

                var aiService = scope.ServiceProvider.GetRequiredService<IAIService>();

                var request = new AIRequest
                {
                    Prompt = job.Prompt
                };

                var response = await aiService.GenerateAsync(request);

                job.Result = response.Output;
                job.Status = "Completed";
                job.CompletedAt = DateTime.UtcNow;

                _jobStore.Update(job);
            }
            catch (Exception ex)
            {
                job.RetryCount++;

                _logger.LogWarning(ex,
                    "Job {JobId} failed. Retry {RetryCount}/{MaxRetries}",
                     job.Id, job.RetryCount, job.MaxRetries);

                if (job.RetryCount < job.MaxRetries)
                {
                    job.Status = "Retrying";

                    _jobStore.Update(job);

                    _queue.Enqueue(job);
                }
                else
                {
                    job.Status = "Failed";
                    job.Error = $"Job failed after {job.RetryCount} attempts: {ex.Message}";
                    job.CompletedAt = DateTime.UtcNow;

                    _jobStore.Update(job);

                    _logger.LogError(ex, "Job {JobId} failed after max retries", job.Id);
                }
            }
        }
    }
}