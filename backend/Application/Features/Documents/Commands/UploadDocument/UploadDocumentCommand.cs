using Application.DTOs.Documents;
using Application.Features.Documents.Models;
using MediatR;

namespace Application.Features.Documents.Commands.UploadDocument;

public class UploadDocumentCommand : IRequest<DocumentResponse>
{
    public DocumentUpload Document { get; set; } = null!;
}