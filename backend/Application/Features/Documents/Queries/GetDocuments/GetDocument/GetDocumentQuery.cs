using MediatR;

namespace Application.Features.Documents.Queries.GetDocument;

public class GetDocumentQuery : IRequest<GetDocumentResult>
{
    public Guid Id { get; set; }
}