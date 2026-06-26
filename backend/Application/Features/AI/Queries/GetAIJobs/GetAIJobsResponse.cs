namespace Application.Features.AI.Queries.GetAIJobs;

public class GetAIJobsResponse
{
    public Guid Id { get; set; }

    public string JobType { get; set; } = string.Empty;

    public string Status { get; set; } = string.Empty;

    public string Prompt { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; }

    public DateTime? CompletedAt { get; set; }
}