using Core.Enums;

namespace Application.Features.AI.Queries.GetAIStatus;

public class AIStatusResponse
{
    public Guid Id { get; set; }

    public string JobType { get; set; } = string.Empty;

    public string Prompt { get; set; } = string.Empty;

    public AIJobStatus Status { get; set; }

    public string? Result { get; set; }

    public string? Error { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? CompletedAt { get; set; }
}