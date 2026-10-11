namespace Application.Features.AI.Queries.GetDocumentSummary;

public class DocumentSummaryResponse
{
    public Guid DocumentId { get; set; }

    public string? Summary { get; set; }

    public DateTime? GeneratedAt { get; set; }
}