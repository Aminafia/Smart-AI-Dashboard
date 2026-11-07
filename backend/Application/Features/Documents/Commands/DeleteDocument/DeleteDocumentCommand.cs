using MediatR;

namespace Application.Features.Documents.Commands.DeleteDocument;

public class DeleteDocumentCommand : IRequest
{
    public Guid Id { get; set; }
}