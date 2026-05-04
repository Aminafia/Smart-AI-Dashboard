using MediatR;

namespace Application.Features.AI.Queries.GetAIStatus;
public class GetAIStatusQuery : IRequest<AIStatusResponse>
{
    public Guid JobId { get; set; }
}