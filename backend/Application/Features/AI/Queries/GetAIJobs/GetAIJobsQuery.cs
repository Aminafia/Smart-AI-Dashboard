using MediatR;
using Application.Common.Models;

namespace Application.Features.AI.Queries.GetAIJobs;

public class GetAIJobsQuery
    : IRequest<PagedResponse<GetAIJobsResponse>>
{
    public int Page { get; set; } = 1;

    public int PageSize { get; set; } = 10;
}