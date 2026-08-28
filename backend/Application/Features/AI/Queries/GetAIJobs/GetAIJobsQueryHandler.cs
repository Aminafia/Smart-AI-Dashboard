using Application.Interfaces;
using Application.Common.Models;
using MediatR;

namespace Application.Features.AI.Queries.GetAIJobs;

public class GetAIJobsQueryHandler : IRequestHandler<GetAIJobsQuery, PagedResponse<GetAIJobsResponse>>
{
    private readonly IAIJobStore _jobStore;
    private readonly ICurrentUserService _currentUser;

    public GetAIJobsQueryHandler(IAIJobStore jobStore, ICurrentUserService currentUser)
    {
        _jobStore = jobStore;
        _currentUser = currentUser;
    }

    public async Task<PagedResponse<GetAIJobsResponse>> Handle(GetAIJobsQuery request, CancellationToken cancellationToken)
    {
        var pagedJobs = await _jobStore.GetJobsAsync(request.Page, request.PageSize, _currentUser.UserId);
        return new PagedResponse<GetAIJobsResponse>
        {
            Items = pagedJobs.Items.Select(job => new GetAIJobsResponse
            {
                Id = job.Id, JobType = job.JobType.ToString(), Status = job.Status,
                Prompt = job.Prompt, CreatedAt = job.CreatedAt, CompletedAt = job.CompletedAt
            }).ToList(),
            Page = pagedJobs.Page, PageSize = pagedJobs.PageSize, TotalCount = pagedJobs.TotalCount
        };
    }
}