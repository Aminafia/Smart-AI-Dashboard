using MediatR;

namespace Application.Features.AI.Queries.GetAIJobs;

public class GetAIJobsQuery
    : IRequest<List<GetAIJobsResponse>>
{
    public int Page { get; set; } = 1;

    public int PageSize { get; set; } = 10;
}