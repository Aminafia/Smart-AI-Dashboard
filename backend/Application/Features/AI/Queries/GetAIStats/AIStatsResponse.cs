namespace Application.Features.AI.Queries.GetAIStats;

public class AIStatsResponse
{
    public int TotalJobs { get; set; }

    public int CompletedJobs { get; set; }

    public int ProcessingJobs { get; set; }

    public int FailedJobs { get; set; }
}