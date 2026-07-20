using Core.Enums;

namespace Application.Features.AI.Queries.GetAIJobs;

public class GetAIJobsResponse
{
    public Guid Id { get; set; }

    public string JobType { get; set; } = string.Empty;

    public AIJobStatus Status { get; set; }

    public string Prompt { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; }

    public DateTime? CompletedAt { get; set; }
}