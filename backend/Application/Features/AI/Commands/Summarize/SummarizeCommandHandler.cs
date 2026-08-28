using Application.DTOs.AI;
using Application.Interfaces;
using Core.Entities;
using Core.Enums;
using MediatR;

namespace Application.Features.AI.Commands.Summarize;

public class SummarizeCommandHandler : IRequestHandler<SummarizeCommand, AIOperationResponse>
{
    private readonly IAIService _aiService;
    private readonly IAIJobStore _jobStore;
    private readonly ICurrentUserService _currentUser;

    public SummarizeCommandHandler(IAIService aiService, IAIJobStore jobStore, ICurrentUserService currentUser)
    {
        _aiService = aiService; _jobStore = jobStore; _currentUser = currentUser;
    }

    public async Task<AIOperationResponse> Handle(SummarizeCommand request, CancellationToken cancellationToken)
    {
        var job = new AIJob
        {
            UserId = _currentUser.UserId, ProjectId = Guid.NewGuid(), JobType = AIJobType.Summarize,
            Prompt = request.Text, Status = AIJobStatus.Processing, CreatedAt = DateTime.UtcNow
        };
        await _jobStore.AddJobAsync(job);

        try
        {
            var response = await _aiService.ProcessAsync(new AIRequest { Input = request.Text, JobType = AIJobType.Summarize });
            job.Status = AIJobStatus.Completed; job.Result = response.Output; job.CompletedAt = DateTime.UtcNow;
            await _jobStore.UpdateJobAsync(job);
            return new AIOperationResponse { JobId = job.Id, Status = job.Status, Output = response.Output, IsFallback = response.IsFallback };
        }
        catch (Exception ex)
        {
            job.Status = AIJobStatus.Failed; job.Error = ex.Message; job.CompletedAt = DateTime.UtcNow;
            await _jobStore.UpdateJobAsync(job); throw;
        }
    }
}