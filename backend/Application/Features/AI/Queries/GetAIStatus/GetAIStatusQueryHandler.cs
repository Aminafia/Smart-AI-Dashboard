using Application.Common.Exceptions;
using Application.Interfaces;
using MediatR;

namespace Application.Features.AI.Queries.GetAIStatus;
public class GetAIStatusQueryHandler 
    : IRequestHandler<GetAIStatusQuery, AIStatusResponse>
{
    private readonly IAIJobStore _jobStore;

    public GetAIStatusQueryHandler(IAIJobStore jobStore)
    {
        _jobStore = jobStore;
    }

    public async Task<AIStatusResponse> Handle(
        GetAIStatusQuery request,
        CancellationToken cancellationToken)
    {
        var job = await _jobStore.GetJobAsync(request.JobId);

        if (job is null)
            throw new NotFoundException("Job not found");

        return new AIStatusResponse
        {
            Id = job.Id,
            Status = job.Status,
            Result = job.Result,
            Error = job.Error
        };
    }
}