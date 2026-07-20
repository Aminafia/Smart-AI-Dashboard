/*
GenerateTextCommandHandler Working
1. Take GenerateTextCommand from controller through MediatR
2. Create new AIJob
3. Save this AIJob to DB through IAIJobStore
4. Enqueue this AIJob to IAIQueue for processing by AIWorker
5. Return AIOperationResponse with JobId and Status to controller
*/

using Application.Interfaces;
using Application.DTOs.AI;
using Core.Entities;
using Core.Enums;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Features.AI.Commands.GenerateText;
public class GenerateTextCommandHandler 
    : IRequestHandler<GenerateTextCommand, AIOperationResponse>
{
    private readonly IAIQueue _queue;
    private readonly IAIJobStore _jobStore;
    private readonly ILogger<GenerateTextCommandHandler> _logger;

    public GenerateTextCommandHandler(
        IAIQueue queue,
        IAIJobStore jobStore,
        ILogger<GenerateTextCommandHandler> logger)
    {
        _queue = queue;
        _jobStore = jobStore;
        _logger = logger;
    }

    public async Task<AIOperationResponse> Handle(GenerateTextCommand request, CancellationToken cancellationToken)
    {
        var job = new AIJob
        {
            ProjectId = Guid.NewGuid(),
            JobType = AIJobType.GenerateText,
            Prompt = request.Prompt
        };

        await _jobStore.AddJobAsync(job);
        _queue.Enqueue(job);

        _logger.LogInformation("[Handler] AI job created: {JobId}", job.Id);

        return new AIOperationResponse
        {
            JobId = job.Id,
            Status = job.Status,
            Output = string.Empty,
            IsFallback = false
        };
    }
}