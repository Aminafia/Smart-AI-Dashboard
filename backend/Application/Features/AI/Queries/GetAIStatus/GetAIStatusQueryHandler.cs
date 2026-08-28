using Application.Common.Exceptions;
using Application.Interfaces;
using MediatR;

namespace Application.Features.AI.Queries.GetAIStatus;

public class GetAIStatusQueryHandler : IRequestHandler<GetAIStatusQuery, AIStatusResponse>
{
    private readonly IAIJobStore _jobStore;
    private readonly ICurrentUserService _currentUser;

    public GetAIStatusQueryHandler(IAIJobStore jobStore, ICurrentUserService currentUser)
    {
        _jobStore = jobStore;
        _currentUser = currentUser;
    }

    public async Task<AIStatusResponse> Handle(GetAIStatusQuery request, CancellationToken cancellationToken)
    {
        var job = await _jobStore.GetJobAsync(request.JobId, _currentUser.UserId);
        if (job is null) throw new NotFoundException("Job not found");

        return new AIStatusResponse
        {
            Id = job.Id, JobType = job.JobType.ToString(), Prompt = job.Prompt,
            Status = job.Status, Result = job.Result, Error = job.Error,
            CreatedAt = job.CreatedAt, CompletedAt = job.CompletedAt
        };
    }
}