using Application.Interfaces;
using Core.Enums;
using MediatR;

namespace Application.Features.AI.Queries.GetAIStats;

public class GetAIStatsQueryHandler
    : IRequestHandler<GetAIStatsQuery, AIStatsResponse>
{
    private readonly IAIJobStore _jobStore;
    private readonly ICurrentUserService _currentUser;

    public GetAIStatsQueryHandler(
        IAIJobStore jobStore,
        ICurrentUserService currentUser)
    {
        _jobStore = jobStore;
        _currentUser = currentUser;
    }

    public async Task<AIStatsResponse> Handle(
        GetAIStatsQuery request,
        CancellationToken cancellationToken)
    {
        return await _jobStore.GetStatsAsync(_currentUser.UserId);
    }
}