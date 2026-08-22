using Application.Common.Exceptions;
using Application.Features.Documents.Models;
using Application.Interfaces;
using MediatR;

namespace Application.Features.Documents.Queries.GetDocumentContent;

public class GetDocumentContentQueryHandler
    : IRequestHandler<GetDocumentContentQuery, DocumentContentResponse>
{
    private readonly IDocumentContentRepository _documentContentRepository;

    public GetDocumentContentQueryHandler(
        IDocumentContentRepository documentContentRepository)
    {
        _documentContentRepository = documentContentRepository;
    }

    public async Task<DocumentContentResponse> Handle(
        GetDocumentContentQuery request,
        CancellationToken cancellationToken)
    {
        var content =
            await _documentContentRepository.GetByDocumentIdAsync(
                request.DocumentId,
                cancellationToken);

        if (content == null)
        {
            throw new NotFoundException(
                "Extracted content not found for this document.");
        }

        return new DocumentContentResponse
        {
            DocumentId = content.DocumentId,
            ExtractedText = content.ExtractedText,
            ExtractedAt = content.ExtractedAt
        };
    }
}