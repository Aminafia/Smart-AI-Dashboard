using Application.Common.Exceptions;
using Application.Features.Documents.Commands.ExtractDocument;
using Application.Interfaces;

using Core.Entities;

using Moq;

namespace Application.Tests.Features.Documents;

public class ExtractDocumentCommandHandlerTests
{
    private readonly Mock<IDocumentRepository> _documents = new();
    private readonly Mock<IDocumentStorage> _storage = new();
    private readonly Mock<IDocumentTextExtractor> _extractor = new();
    private readonly Mock<IDocumentContentRepository> _content = new();
    private readonly Mock<ICurrentUserService> _currentUser = new();

    [Fact]
    public async Task Handle_DocumentNotFound_ThrowsNotFoundWithoutOpeningStorage()
    {
        var id = Guid.NewGuid();
        _currentUser.SetupGet(x => x.UserId).Returns(Guid.NewGuid());
        _documents.Setup(x => x.GetByIdAsync(id, It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Document?)null);

        await Assert.ThrowsAsync<NotFoundException>(() =>
            CreateHandler().Handle(new ExtractDocumentCommand { DocumentId = id }, CancellationToken.None));

        _storage.Verify(x => x.OpenReadAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_NoExistingContent_AddsExtractedText()
    {
        var id = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var document = new Document { Id = id, UserId = userId, StoragePath = "report.pdf", ContentType = "application/pdf" };
        using var stream = new MemoryStream([1, 2, 3]);
        DocumentContent? added = null;
        _currentUser.SetupGet(x => x.UserId).Returns(userId);
        _documents.Setup(x => x.GetByIdAsync(id, userId, It.IsAny<CancellationToken>())).ReturnsAsync(document);
        _storage.Setup(x => x.OpenReadAsync(document.StoragePath, It.IsAny<CancellationToken>()))
            .ReturnsAsync(stream);
        _extractor.Setup(x => x.ExtractTextAsync(stream, document.ContentType, It.IsAny<CancellationToken>()))
            .ReturnsAsync("Extracted document text");
        _content.Setup(x => x.GetByDocumentIdAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync((DocumentContent?)null);
        _content.Setup(x => x.AddAsync(It.IsAny<DocumentContent>(), It.IsAny<CancellationToken>()))
            .Callback<DocumentContent, CancellationToken>((value, _) => added = value)
            .Returns(Task.CompletedTask);

        await CreateHandler().Handle(new ExtractDocumentCommand { DocumentId = id }, CancellationToken.None);

        Assert.NotNull(added);
        Assert.Equal(id, added.DocumentId);
        Assert.Equal("Extracted document text", added.ExtractedText);
        Assert.NotEqual(Guid.Empty, added.Id);
        _content.Verify(x => x.AddAsync(added, It.IsAny<CancellationToken>()), Times.Once);
        _content.Verify(x => x.UpdateAsync(It.IsAny<DocumentContent>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ExistingContent_UpdatesInsteadOfAddingDuplicate()
    {
        var id = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var document = new Document { Id = id, UserId = userId, StoragePath = "report.pdf", ContentType = "application/pdf" };
        var existing = new DocumentContent { Id = Guid.NewGuid(), DocumentId = id, ExtractedText = "Old text" };
        using var stream = new MemoryStream([4, 5, 6]);
        _currentUser.SetupGet(x => x.UserId).Returns(userId);
        _documents.Setup(x => x.GetByIdAsync(id, userId, It.IsAny<CancellationToken>())).ReturnsAsync(document);
        _storage.Setup(x => x.OpenReadAsync(document.StoragePath, It.IsAny<CancellationToken>()))
            .ReturnsAsync(stream);
        _extractor.Setup(x => x.ExtractTextAsync(stream, document.ContentType, It.IsAny<CancellationToken>()))
            .ReturnsAsync("Updated text");
        _content.Setup(x => x.GetByDocumentIdAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existing);
        _content.Setup(x => x.UpdateAsync(existing, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        await CreateHandler().Handle(new ExtractDocumentCommand { DocumentId = id }, CancellationToken.None);

        Assert.Equal("Updated text", existing.ExtractedText);
        _content.Verify(x => x.UpdateAsync(existing, It.IsAny<CancellationToken>()), Times.Once);
        _content.Verify(x => x.AddAsync(It.IsAny<DocumentContent>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    private ExtractDocumentCommandHandler CreateHandler() =>
        new(_documents.Object, _storage.Object, _extractor.Object, _content.Object, _currentUser.Object);
}
