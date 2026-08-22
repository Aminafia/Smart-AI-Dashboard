namespace Core.Entities;

public class DocumentContent
{
    public Guid Id { get; set; }

    public Guid DocumentId { get; set; }

    public string ExtractedText { get; set; } = string.Empty;

    public DateTime ExtractedAt { get; set; } = DateTime.UtcNow;

    public Document Document { get; set; } = null!;
}