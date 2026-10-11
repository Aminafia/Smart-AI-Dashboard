using Application.Interfaces;
using Core.Enums;
using MediatR;

namespace Application.Features.AI.Queries.GetDocumentSummary;

public class GetDocumentSummaryQueryHandler
    : IRequestHandler<GetDocumentSummaryQuery, DocumentSummaryResponse>
{
    private readonly IAIJobStore _jobStore;
    private readonly ICurrentUserService _currentUser;

    public GetDocumentSummaryQueryHandler(
        IAIJobStore jobStore,
        ICurrentUserService currentUser)
    {
        _jobStore = jobStore;
        _currentUser = currentUser;
    }

    public async Task<DocumentSummaryResponse> Handle(
        GetDocumentSummaryQuery request,
        CancellationToken cancellationToken)
    {
        var job = await _jobStore.GetLatestCompletedSummaryAsync(
            request.DocumentId,
            _currentUser.UserId);

        if (job is null)
        {
            return new DocumentSummaryResponse
            {
                DocumentId = request.DocumentId
            };
        }

        return new DocumentSummaryResponse
        {
            DocumentId = request.DocumentId,
            Summary = job.Result,
            GeneratedAt = job.CompletedAt
        };
    }
}