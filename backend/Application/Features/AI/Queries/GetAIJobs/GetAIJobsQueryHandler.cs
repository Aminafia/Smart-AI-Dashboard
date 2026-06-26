using Application.Interfaces;
using MediatR;

namespace Application.Features.AI.Queries.GetAIJobs;

public class GetAIJobsQueryHandler
    : IRequestHandler<
        GetAIJobsQuery,
        List<GetAIJobsResponse>>
{
    private readonly IAIJobStore _jobStore;

    public GetAIJobsQueryHandler(
        IAIJobStore jobStore)
    {
        _jobStore = jobStore;
    }

    public async Task<List<GetAIJobsResponse>> Handle(
        GetAIJobsQuery request,
        CancellationToken cancellationToken)
    {
        var jobs = await _jobStore.GetJobsAsync(request.Page, request.PageSize);

        return jobs.Select(job =>
            new GetAIJobsResponse
            {
                Id = job.Id,
                JobType = job.JobType.ToString(),
                Status = job.Status,
                Prompt = job.Prompt,
                CreatedAt = job.CreatedAt,
                CompletedAt = job.CompletedAt
            })
            .ToList();
    }
}