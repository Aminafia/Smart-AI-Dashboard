using Application.Interfaces;
using Core.Entities;
using MediatR;
using Microsoft.Extensions.Logging;
using Application.Features.AI.Commands.GenerateAI;
public class GenerateAICommandHandler 
    : IRequestHandler<GenerateAICommand, GenerateAIResponse>
{
    private readonly IAIQueue _queue;
    private readonly IAIJobStore _jobStore;
    private readonly ILogger<GenerateAICommandHandler> _logger;

    public GenerateAICommandHandler(
        IAIQueue queue,
        IAIJobStore jobStore,
        ILogger<GenerateAICommandHandler> logger)
    {
        _queue = queue;
        _jobStore = jobStore;
        _logger = logger;
    }

    public Task<GenerateAIResponse> Handle(
        GenerateAICommand request,
        CancellationToken cancellationToken)
    {
        var job = new AIJob
        {
            ProjectId = Guid.NewGuid(),
            JobType = "Generate",
            Prompt = request.Prompt
        };

        _jobStore.Add(job);
        _queue.Enqueue(job);

        _logger.LogInformation("[Handler] AI job created: {JobId}", job.Id);

        return Task.FromResult(new GenerateAIResponse
        {
            JobId = job.Id,
            Status = job.Status
        });
    }
}