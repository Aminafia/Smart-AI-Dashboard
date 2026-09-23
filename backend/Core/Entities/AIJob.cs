using Core.Enums;

namespace Core.Entities;

public class AIJob
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid UserId { get; set; }

    public Guid ProjectId { get; set; }

    public Guid? DocumentId { get; set; }

    public AIJobType JobType { get; set; }

    public string Prompt { get; set; } = string.Empty;

    public string? Result { get; set; }

    public AIJobStatus Status { get; set; } = AIJobStatus.Pending;

    public string? Error { get; set; }

    public int RetryCount { get; set; }

    public int MaxRetries { get; set; } = 3;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? CompletedAt { get; set; }
}