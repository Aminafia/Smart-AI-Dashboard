using Application.DTOs.Documents;
using MediatR;

namespace Application.Features.Documents.Commands.UploadDocument;

public class UploadDocumentCommand : IRequest<UploadDocumentResponse>
{
    public DocumentUpload Document { get; set; } = null!;
}