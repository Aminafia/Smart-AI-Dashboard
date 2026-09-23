using Application.Common.Exceptions;
using Application.DTOs.AI;
using Application.Interfaces;
using Core.Entities;
using Core.Enums;
using MediatR;

namespace Application.Features.AI.Commands.SummarizeDocument;

public class SummarizeDocumentCommandHandler : IRequestHandler<SummarizeDocumentCommand, AIOperationResponse>
{
    private readonly IDocumentContentRepository _documentContentRepository;
    private readonly IDocumentRepository _documentRepository;
    private readonly IAIService _aiService;
    private readonly IAIJobStore _jobStore;
    private readonly ICurrentUserService _currentUser;

    public SummarizeDocumentCommandHandler(IDocumentContentRepository documentContentRepository, IDocumentRepository documentRepository,
        IAIService aiService, IAIJobStore jobStore, ICurrentUserService currentUser)
    {
        _documentContentRepository = documentContentRepository; _documentRepository = documentRepository;
        _aiService = aiService; _jobStore = jobStore; _currentUser = currentUser;
    }

    public async Task<AIOperationResponse> Handle(SummarizeDocumentCommand request, CancellationToken cancellationToken)
    {
        var document = await _documentRepository.GetByIdAsync(request.DocumentId, _currentUser.UserId, cancellationToken);
        if (document == null) throw new NotFoundException("Document not found.");

        var content = await _documentContentRepository.GetByDocumentIdAsync(request.DocumentId, cancellationToken);
        if (content == null || string.IsNullOrWhiteSpace(content.ExtractedText))
            throw new NotFoundException("Extracted content not found for this document. Extract the document before summarizing it.");

        var job = new AIJob
        {
            UserId = _currentUser.UserId, DocumentId = document.Id, ProjectId = Guid.NewGuid(),
            JobType = AIJobType.Summarize, Prompt = content.ExtractedText,
            Status = AIJobStatus.Processing, CreatedAt = DateTime.UtcNow
        };
        await _jobStore.AddJobAsync(job);

        try
        {
            var response = await _aiService.ProcessAsync(new AIRequest { Input = content.ExtractedText, JobType = AIJobType.Summarize });
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