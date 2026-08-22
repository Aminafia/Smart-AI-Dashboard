using MediatR;

namespace Application.Features.Documents.Commands.ExtractDocument;

public class ExtractDocumentCommand : IRequest
{
    public Guid DocumentId { get; set; }
}