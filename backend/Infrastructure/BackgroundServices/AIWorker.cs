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
    private readonly ILogger<AIWorker> _logger;

    public AIWorker(
        IAIQueue queue,
        IServiceScopeFactory scopeFactory,
        ILogger<AIWorker> logger)
    {
        _queue = queue;
        _scopeFactory = scopeFactory;
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

            using var scope = _scopeFactory.CreateScope();

            var jobStore =
                scope.ServiceProvider.GetRequiredService<IAIJobStore>();

            var aiService =
                scope.ServiceProvider.GetRequiredService<IAIService>();

            try
            {
                job.Status = "Processing";

                await jobStore.UpdateJobAsync(job);

                var request = new AIRequest
                {
                    Prompt = job.Prompt
                };

                var response =
                    await aiService.GenerateAsync(request);

                job.Result = response.Output;
                job.Status = "Completed";
                job.CompletedAt = DateTime.UtcNow;

                await jobStore.UpdateJobAsync(job);
            }
            catch (Exception ex)
            {
                job.RetryCount++;

                _logger.LogWarning(
                    ex,
                    "Job {JobId} failed. Retry {RetryCount}/{MaxRetries}",
                    job.Id,
                    job.RetryCount,
                    job.MaxRetries);

                if (job.RetryCount < job.MaxRetries)
                {
                    job.Status = "Retrying";

                    await jobStore.UpdateJobAsync(job);

                    _queue.Enqueue(job);
                }
                else
                {
                    job.Status = "Failed";

                    job.Error =
                        $"Job failed after {job.RetryCount} attempts: {ex.Message}";

                    job.CompletedAt = DateTime.UtcNow;

                    await jobStore.UpdateJobAsync(job);

                    _logger.LogError(
                        ex,
                        "Job {JobId} failed after max retries",
                        job.Id);
                }
            }
        }
    }
}