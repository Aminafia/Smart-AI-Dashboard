using Application.Features.Documents.Models;
using MediatR;

namespace Application.Features.Documents.Queries.GetDocumentContent;

public class GetDocumentContentQuery
    : IRequest<DocumentContentResponse>
{
    public Guid DocumentId { get; set; }
}