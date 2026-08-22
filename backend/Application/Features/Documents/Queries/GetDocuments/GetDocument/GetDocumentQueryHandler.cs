using Application.Common.Exceptions;
using Application.Interfaces;
using MediatR;

namespace Application.Features.Documents.Queries.GetDocument;

public class GetDocumentQueryHandler
    : IRequestHandler<GetDocumentQuery, GetDocumentResult>
{
    private readonly IDocumentRepository _documentRepository;
    private readonly IDocumentStorage _documentStorage;

    public GetDocumentQueryHandler(
        IDocumentRepository documentRepository,
        IDocumentStorage documentStorage)
    {
        _documentRepository = documentRepository;
        _documentStorage = documentStorage;
    }

    public async Task<GetDocumentResult> Handle(
        GetDocumentQuery request,
        CancellationToken cancellationToken)
    {
        var document = await _documentRepository.GetByIdAsync(
            request.Id,
            cancellationToken);

        if (document == null)
        {
            throw new NotFoundException("Document not found.");
        }

        var stream = await _documentStorage.OpenReadAsync(
            document.StoragePath,
            cancellationToken);

        return new GetDocumentResult
        {
            Content = stream,
            FileName = document.FileName,
            ContentType = document.ContentType
        };
    }
}