using MediatR;

namespace Application.Features.AI.Queries.GetDocumentSummary;

public class GetDocumentSummaryQuery : IRequest<DocumentSummaryResponse>
{
    public Guid DocumentId { get; set; }
}