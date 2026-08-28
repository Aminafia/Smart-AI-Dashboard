using Application.Common.Exceptions;
using Application.Interfaces;
using MediatR;

namespace Application.Features.Documents.Commands.DeleteDocument;

public class DeleteDocumentCommandHandler : IRequestHandler<DeleteDocumentCommand>
{
    private readonly IDocumentRepository _documentRepository;
    private readonly IDocumentStorage _documentStorage;
    private readonly ICurrentUserService _currentUser;

    public DeleteDocumentCommandHandler(IDocumentRepository documentRepository, IDocumentStorage documentStorage, ICurrentUserService currentUser)
    {
        _documentRepository = documentRepository;
        _documentStorage = documentStorage;
        _currentUser = currentUser;
    }

    public async Task Handle(DeleteDocumentCommand request, CancellationToken cancellationToken)
    {
        var document = await _documentRepository.GetByIdAsync(request.Id, _currentUser.UserId, cancellationToken);
        if (document == null) throw new NotFoundException("Document not found.");

        await _documentStorage.DeleteAsync(document.StoragePath, cancellationToken);
        await _documentRepository.DeleteAsync(document, cancellationToken);
    }
}