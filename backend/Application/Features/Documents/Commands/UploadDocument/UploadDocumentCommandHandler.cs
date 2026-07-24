using Application.Interfaces;
using Core.Entities;
using MediatR;

namespace Application.Features.Documents.Commands.UploadDocument;

public class UploadDocumentCommandHandler
    : IRequestHandler<UploadDocumentCommand, UploadDocumentResponse>
{
    private readonly IDocumentStorage _documentStorage;
    private readonly IDocumentStore _documentStore;

    public UploadDocumentCommandHandler(
        IDocumentStorage documentStorage,
    IDocumentStore documentStore)
    {
      _documentStorage = documentStorage;
      _documentStore = documentStore;
    }

    public async Task<UploadDocumentResponse> Handle(
        UploadDocumentCommand request,
        CancellationToken cancellationToken)
    {
        var storedFileName = await _documentStorage.SaveAsync(request.Document);

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

        await _documentStore.AddAsync(document);

        return new UploadDocumentResponse
        {
            Id = document.Id,
            FileName = document.FileName,
            ContentType = document.ContentType,
            FileSize = document.FileSize,
            UploadedAt = document.UploadedAt
        };
    }
}