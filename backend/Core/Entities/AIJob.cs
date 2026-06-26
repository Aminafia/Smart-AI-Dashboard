namespace Core.Entities;

using Core.Enums;
public class AIJob
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid ProjectId { get; set; }

    public AIJobType JobType { get; set; }

    public string Prompt { get; set; } = string.Empty;

    public string? Result { get; set; }

    public string Status { get; set; } = "Pending"; // Initially a new AIJob has Pending status

    public string? Error { get; set; }

    public int RetryCount { get; set; } = 0;
    public int MaxRetries { get; set; } = 3;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? CompletedAt { get; set; }
}