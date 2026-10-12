/*
AIWorker Working:
1. Continuously monitor AIQueue for new jobs.
2. Dequeue the next AIJob.
3. Update AIJob Status="Processing" and save to database.
4. Create an AIRequest from the AIJob.
5. Call IAIService.ProcessAsync().
6. Store AI result, update Status="Completed" and save to database.
7. If processing fails:
   - Increase RetryCount.
   - Log the failure.
   - If retries remain:
       • Update status to Retrying.
       • Save to database.
       • Enqueue the job again.
   - Otherwise:
       • Update status to Failed.
       • Save error message and CompletedAt.
       • Save to database.
       • Log the final failure.
*/


using Application.DTOs.AI;
using Application.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Core.Enums;

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
        _logger.LogInformation("AI Worker is starting. Recovering queued jobs.");

        using (var recoveryScope = _scopeFactory.CreateScope())
        {
            var jobStore = recoveryScope.ServiceProvider
                .GetRequiredService<IAIJobStore>();

            var recoverableJobs = await jobStore.GetRecoverableJobsAsync();

            foreach (var job in recoverableJobs)
            {
                if (stoppingToken.IsCancellationRequested)
                {
                    break;
                }


                if (job.RetryCount >= job.MaxRetries)
                {
                    job.Status = AIJobStatus.Failed;

                    job.Error ??=
                        $"Job exhausted its retry allowance during recovery " +
                        $"({job.RetryCount}/{job.MaxRetries}).";

                    job.CompletedAt ??= DateTime.UtcNow;

                    await jobStore.UpdateJobAsync(job);

                    _logger.LogWarning(
                        "Job {JobId} was marked Failed during recovery because " +
                        "its retry allowance was exhausted ({RetryCount}/{MaxRetries}).",
                        job.Id,
                        job.RetryCount,
                        job.MaxRetries);

                    continue;
                }
                if (job.Status == AIJobStatus.Processing)
                {
                    job.Status = AIJobStatus.Pending;
                    await jobStore.UpdateJobAsync(job);
                }

                _queue.Enqueue(job);

                _logger.LogInformation(
                    "Recovered AI job {JobId} with status {Status}",
                    job.Id,
                    job.Status);
            }
        }

        _logger.LogInformation("AI Worker recovery finished.");

        while (!stoppingToken.IsCancellationRequested)

        {
            var job = _queue.Dequeue();

            if (job is null)
            {
                await Task.Delay(500, stoppingToken);
                continue;
            }

            using var scope = _scopeFactory.CreateScope();

            var jobStore = scope.ServiceProvider.GetRequiredService<IAIJobStore>();

            var aiService = scope.ServiceProvider.GetRequiredService<IAIService>();

            try
            {
                job.Status = AIJobStatus.Processing;

                await jobStore.UpdateJobAsync(job);

                var request = new AIRequest
                {
                    Input = job.Prompt,
                    JobType = job.JobType
                };

                var response = await aiService.ProcessAsync(request);

                job.Result = response.Output;
                job.Status = AIJobStatus.Completed;
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
                    job.Status = AIJobStatus.Retrying;

                    await jobStore.UpdateJobAsync(job);

                    var delaySeconds = Math.Pow(2, job.RetryCount);

                    try
                    {
                        await Task.Delay(TimeSpan.FromSeconds(delaySeconds), stoppingToken);
                    }
                    catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
                    {
                        _logger.LogInformation(
                            "Worker shutting down while job {JobId} was waiting to retry.",
                            job.Id);

                        break;
                    }

                    _queue.Enqueue(job);
                }
                else
                {
                    job.Status = AIJobStatus.Failed;

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