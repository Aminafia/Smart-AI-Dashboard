using Application.DTOs.AI;
using Application.Interfaces;
using Core.Entities;
using Core.Enums;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Features.AI.Commands.GenerateText;

public class GenerateTextCommandHandler : IRequestHandler<GenerateTextCommand, AIOperationResponse>
{
    private readonly IAIQueue _queue;
    private readonly IAIJobStore _jobStore;
    private readonly ICurrentUserService _currentUser;
    private readonly ILogger<GenerateTextCommandHandler> _logger;

    public GenerateTextCommandHandler(IAIQueue queue, IAIJobStore jobStore, ICurrentUserService currentUser, ILogger<GenerateTextCommandHandler> logger)
    {
        _queue = queue; _jobStore = jobStore; _currentUser = currentUser; _logger = logger;
    }

    public async Task<AIOperationResponse> Handle(GenerateTextCommand request, CancellationToken cancellationToken)
    {
        var job = new AIJob { UserId = _currentUser.UserId, ProjectId = Guid.NewGuid(), JobType = AIJobType.GenerateText, Prompt = request.Prompt };
        await _jobStore.AddJobAsync(job);
        _queue.Enqueue(job);
        _logger.LogInformation("AI job created: {JobId}", job.Id);
        return new AIOperationResponse { JobId = job.Id, Status = job.Status, Output = string.Empty, IsFallback = false };
    }
}