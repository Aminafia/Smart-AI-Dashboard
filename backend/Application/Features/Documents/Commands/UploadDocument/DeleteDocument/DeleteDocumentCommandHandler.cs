using Application.Common.Exceptions;
using Application.Interfaces;
using MediatR;

namespace Application.Features.Documents.Commands.DeleteDocument;

public class DeleteDocumentCommandHandler
    : IRequestHandler<DeleteDocumentCommand>
{
    private readonly IDocumentRepository _documentRepository;
    private readonly IDocumentStorage _documentStorage;

    public DeleteDocumentCommandHandler(
        IDocumentRepository documentRepository,
        IDocumentStorage documentStorage)
    {
        _documentRepository = documentRepository;
        _documentStorage = documentStorage;
    }

    public async Task Handle(
        DeleteDocumentCommand request,
        CancellationToken cancellationToken)
    {
        var document = await _documentRepository.GetByIdAsync(request.Id);

        if (document == null)
        {
            throw new NotFoundException("Document not found.");
        }

        await _documentStorage.DeleteAsync(document.StoragePath);

        await _documentRepository.DeleteAsync(document);
    }
}