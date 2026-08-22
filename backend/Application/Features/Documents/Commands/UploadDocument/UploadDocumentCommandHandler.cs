using Application.Features.Documents.Models;
using Application.Interfaces;
using Core.Entities;
using MediatR;

namespace Application.Features.Documents.Commands.UploadDocument;

public class UploadDocumentCommandHandler
    : IRequestHandler<UploadDocumentCommand, DocumentResponse>
{
    private readonly IDocumentStorage _documentStorage;
    private readonly IDocumentRepository _documentRepository;

    public UploadDocumentCommandHandler(
        IDocumentStorage documentStorage,
        IDocumentRepository documentRepository)
    {
        _documentStorage = documentStorage;
        _documentRepository = documentRepository;
    }

    public async Task<DocumentResponse> Handle(
        UploadDocumentCommand request,
        CancellationToken cancellationToken)
    {
        var storedFileName =
            await _documentStorage.SaveAsync(
                request.Document,
                cancellationToken);

        try
        {
            var document = new Document
            {
                Id = Guid.NewGuid(),
                FileName = request.Document.FileName,
                StoredFileName = storedFileName,
                StoragePath = storedFileName,
                ContentType = request.Document.ContentType,
                FileSize = request.Document.Content.Length,
                UploadedAt = DateTime.UtcNow
            };

            await _documentRepository.AddAsync(
                document,
                cancellationToken);

            return new DocumentResponse
            {
                Id = document.Id,
                FileName = document.FileName,
                ContentType = document.ContentType,
                FileSize = document.FileSize,
                UploadedAt = document.UploadedAt
            };
        }
        catch
        {
            await _documentStorage.DeleteAsync(
                storedFileName,
                CancellationToken.None);

            throw;
        }
    }
}