namespace Application.Features.Documents.Models;

public class DocumentContentResponse
{
    public Guid DocumentId { get; set; }

    public string ExtractedText { get; set; } = string.Empty;

    public DateTime ExtractedAt { get; set; }
}