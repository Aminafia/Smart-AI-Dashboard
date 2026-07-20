using Core.Enums;

namespace Application.Features.AI.Queries.GetAIStatus;

public class AIStatusResponse
{
    public Guid Id { get; set; }
    public AIJobStatus Status { get; set; }
    public string? Result { get; set; }
    public string? Error { get; set; }
}