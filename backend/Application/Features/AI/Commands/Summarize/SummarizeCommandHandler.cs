/*
SummarizeCommandHandler Working:
1. Take SummarizeCommand from controller through MediatR
2. Create new AIRequest with JobType=Summarize
3. Call IAIService.ProcessAsync() with this AIRequest
4. Convert AIProviderResponse -> AIOperationResponse 
5. Return AIOperationResponse to controller
*/

using Application.DTOs.AI;
using Application.Interfaces;
using MediatR;
using Core.Enums;
using Core.Entities;

namespace Application.Features.AI.Commands.Summarize;

public class SummarizeCommandHandler
    : IRequestHandler<SummarizeCommand, AIOperationResponse>
{
    private readonly IAIService _aiService;
    private readonly IAIJobStore _jobStore;

    public SummarizeCommandHandler(IAIService aiService, IAIJobStore jobStore)
    {
        _aiService = aiService;
        _jobStore = jobStore;
    }

    public async Task<AIOperationResponse> Handle(SummarizeCommand request, CancellationToken cancellationToken)
    {

        var job = new AIJob
        {
            ProjectId = Guid.NewGuid(),
            JobType = AIJobType.Summarize,
            Prompt = request.Text,
            Status = AIJobStatus.Processing,
            CreatedAt = DateTime.UtcNow
        };

        await _jobStore.AddJobAsync(job);

        try
        {
            var aiRequest = new AIRequest
            {
                Input = request.Text,
                JobType = AIJobType.Summarize
            };

            var providerResponse = await _aiService.ProcessAsync(aiRequest);

            job.Status = AIJobStatus.Completed;
            job.Result = providerResponse.Output;
            job.CompletedAt = DateTime.UtcNow;

            await _jobStore.UpdateJobAsync(job);

            return new AIOperationResponse
            {
                JobId = job.Id,
                Status = job.Status,
                Output = providerResponse.Output,
                IsFallback = providerResponse.IsFallback
            };
        }

        catch (Exception ex)
        {
            job.Status = AIJobStatus.Failed;
            job.Error = ex.Message;
            job.CompletedAt = DateTime.UtcNow;

            await _jobStore.UpdateJobAsync(job);

            throw;
        }


    }
}