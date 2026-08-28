using Application.Common.Exceptions;
using Application.Features.Documents.Models;
using Application.Interfaces;
using MediatR;

namespace Application.Features.Documents.Queries.GetDocumentContent;

public class GetDocumentContentQueryHandler : IRequestHandler<GetDocumentContentQuery, DocumentContentResponse>
{
    private readonly IDocumentContentRepository _documentContentRepository;
    private readonly IDocumentRepository _documentRepository;
    private readonly ICurrentUserService _currentUser;

    public GetDocumentContentQueryHandler(IDocumentContentRepository documentContentRepository, IDocumentRepository documentRepository, ICurrentUserService currentUser)
    {
        _documentContentRepository = documentContentRepository;
        _documentRepository = documentRepository;
        _currentUser = currentUser;
    }

    public async Task<DocumentContentResponse> Handle(GetDocumentContentQuery request, CancellationToken cancellationToken)
    {
        var document = await _documentRepository.GetByIdAsync(request.DocumentId, _currentUser.UserId, cancellationToken);
        if (document == null) throw new NotFoundException("Document not found.");

        var content = await _documentContentRepository.GetByDocumentIdAsync(request.DocumentId, cancellationToken);
        if (content == null) throw new NotFoundException("Extracted content not found for this document.");

        return new DocumentContentResponse { DocumentId = content.DocumentId, ExtractedText = content.ExtractedText, ExtractedAt = content.ExtractedAt };
    }
}