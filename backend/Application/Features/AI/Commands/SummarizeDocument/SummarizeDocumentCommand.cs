using Application.DTOs.AI;
using MediatR;

namespace Application.Features.AI.Commands.SummarizeDocument;

public class SummarizeDocumentCommand : IRequest<AIOperationResponse>
{
    public Guid DocumentId { get; set; }
}
