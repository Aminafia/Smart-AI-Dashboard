namespace Core.Entities
{
    public class AIJob
    {
        public Guid Id { get; set; }
        public Guid ProjectId { get; set; }
        public string JobType { get; set; } = default!; // Summarization, Predict, Analyze
        public string Status { get; set; } = "Pending";
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? CompletedAt { get; set; }
    }
}   