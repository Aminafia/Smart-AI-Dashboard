using Application.Common.Exceptions;
using Application.DTOs.AI;
using Application.Interfaces;
using Core.Entities;
using Core.Enums;
using MediatR;

namespace Application.Features.AI.Commands.SummarizeDocument;

public class SummarizeDocumentCommandHandler
    : IRequestHandler<SummarizeDocumentCommand, AIOperationResponse>
{
    private readonly IDocumentContentRepository _documentContentRepository;
    private readonly IAIService _aiService;
    private readonly IAIJobStore _jobStore;

    public SummarizeDocumentCommandHandler(
        IDocumentContentRepository documentContentRepository,
        IAIService aiService,
        IAIJobStore jobStore)
    {
        _documentContentRepository = documentContentRepository;
        _aiService = aiService;
        _jobStore = jobStore;
    }

    public async Task<AIOperationResponse> Handle(
        SummarizeDocumentCommand request,
        CancellationToken cancellationToken)
    {
        var content = await _documentContentRepository.GetByDocumentIdAsync(
            request.DocumentId,
            cancellationToken);

        if (content == null || string.IsNullOrWhiteSpace(content.ExtractedText))
        {
            throw new NotFoundException(
                "Extracted content not found for this document. Extract the document before summarizing it.");
        }

        var job = new AIJob
        {
            ProjectId = request.DocumentId,
            JobType = AIJobType.Summarize,
            Prompt = content.ExtractedText,
            Status = AIJobStatus.Processing,
            CreatedAt = DateTime.UtcNow
        };

        await _jobStore.AddJobAsync(job);

        try
        {
            var providerResponse = await _aiService.ProcessAsync(new AIRequest
            {
                Input = content.ExtractedText,
                JobType = AIJobType.Summarize
            });

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
