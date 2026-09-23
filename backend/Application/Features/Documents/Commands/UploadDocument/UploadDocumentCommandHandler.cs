using Application.Features.Documents.Models;
using Application.Interfaces;
using Core.Entities;
using MediatR;

namespace Application.Features.Documents.Commands.UploadDocument;

public class UploadDocumentCommandHandler : IRequestHandler<UploadDocumentCommand, DocumentResponse>
{
    private readonly IDocumentStorage _documentStorage;
    private readonly IDocumentRepository _documentRepository;
    private readonly ICurrentUserService _currentUser;

    public UploadDocumentCommandHandler(IDocumentStorage documentStorage, IDocumentRepository documentRepository, ICurrentUserService currentUser)
    {
        _documentStorage = documentStorage;
        _documentRepository = documentRepository;
        _currentUser = currentUser;
    }

    public async Task<DocumentResponse> Handle(UploadDocumentCommand request, CancellationToken cancellationToken)
    {
        var storedFileName = await _documentStorage.SaveAsync(request.Document, cancellationToken);
        try
        {
            var document = new Document
            {
                Id = Guid.NewGuid(), UserId = _currentUser.UserId,
                FileName = request.Document.FileName, StoredFileName = storedFileName,
                StoragePath = storedFileName, ContentType = request.Document.ContentType,
                FileSize = request.Document.Content.Length, UploadedAt = DateTime.UtcNow
            };

            await _documentRepository.AddAsync(document, cancellationToken);
            return new DocumentResponse { Id = document.Id, FileName = document.FileName, ContentType = document.ContentType, FileSize = document.FileSize, UploadedAt = document.UploadedAt };
        }
        catch
        {
            await _documentStorage.DeleteAsync(storedFileName, CancellationToken.None);
            throw;
        }
    }
}