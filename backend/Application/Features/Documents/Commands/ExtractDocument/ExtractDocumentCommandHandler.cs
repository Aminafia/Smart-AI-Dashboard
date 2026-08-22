using Application.Common.Exceptions;
using Application.Interfaces;
using Core.Entities;
using MediatR;

namespace Application.Features.Documents.Commands.ExtractDocument;

public class ExtractDocumentCommandHandler
    : IRequestHandler<ExtractDocumentCommand>
{
    private readonly IDocumentRepository _documentRepository;
    private readonly IDocumentStorage _documentStorage;
    private readonly IDocumentTextExtractor _documentTextExtractor;
    private readonly IDocumentContentRepository _documentContentRepository;

    public ExtractDocumentCommandHandler(
        IDocumentRepository documentRepository,
        IDocumentStorage documentStorage,
        IDocumentTextExtractor documentTextExtractor,
        IDocumentContentRepository documentContentRepository)
    {
        _documentRepository = documentRepository;
        _documentStorage = documentStorage;
        _documentTextExtractor = documentTextExtractor;
        _documentContentRepository = documentContentRepository;
    }

    public async Task Handle(
        ExtractDocumentCommand request,
        CancellationToken cancellationToken)
    {
        var document = await _documentRepository.GetByIdAsync(
            request.DocumentId,
            cancellationToken);

        if (document == null)
        {
            throw new NotFoundException("Document not found.");
        }

        await using var stream =
            await _documentStorage.OpenReadAsync(
                document.StoragePath,
                cancellationToken);

        var extractedText =
            await _documentTextExtractor.ExtractTextAsync(
                stream,
                document.ContentType,
                cancellationToken);

        var existingContent =
            await _documentContentRepository.GetByDocumentIdAsync(
                document.Id,
                cancellationToken);

        if (existingContent == null)
        {
            var documentContent = new DocumentContent
            {
                Id = Guid.NewGuid(),
                DocumentId = document.Id,
                ExtractedText = extractedText,
                ExtractedAt = DateTime.UtcNow
            };

            await _documentContentRepository.AddAsync(
                documentContent,
                cancellationToken);
        }
        else
        {
            existingContent.ExtractedText = extractedText;
            existingContent.ExtractedAt = DateTime.UtcNow;

            await _documentContentRepository.UpdateAsync(
                existingContent,
                cancellationToken);
        }
    }
}