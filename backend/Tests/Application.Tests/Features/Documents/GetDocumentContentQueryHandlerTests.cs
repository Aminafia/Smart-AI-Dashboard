using Application.Common.Exceptions;
using Application.Features.Documents.Queries.GetDocumentContent;
using Application.Interfaces;
using Core.Entities;
using Moq;

namespace Application.Tests.Features.Documents;

public class GetDocumentContentQueryHandlerTests
{
    private readonly Mock<IDocumentContentRepository> _content = new();
    private readonly Mock<IDocumentRepository> _documents = new();
    private readonly Mock<ICurrentUserService> _currentUser = new();

    [Fact]
    public async Task Handle_DocumentNotFound_ThrowsNotFoundBeforeReadingContent()
    {
        var id = Guid.NewGuid();
        _currentUser.SetupGet(x => x.UserId).Returns(Guid.NewGuid());
        _documents.Setup(x => x.GetByIdAsync(id, It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Document?)null);

        await Assert.ThrowsAsync<NotFoundException>(() =>
            CreateHandler().Handle(new GetDocumentContentQuery { DocumentId = id }, CancellationToken.None));

        _content.Verify(x => x.GetByDocumentIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ContentNotFound_ThrowsNotFound()
    {
        var id = Guid.NewGuid();
        var userId = Guid.NewGuid();
        _currentUser.SetupGet(x => x.UserId).Returns(userId);
        _documents.Setup(x => x.GetByIdAsync(id, userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Document { Id = id, UserId = userId });
        _content.Setup(x => x.GetByDocumentIdAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync((DocumentContent?)null);

        await Assert.ThrowsAsync<NotFoundException>(() =>
            CreateHandler().Handle(new GetDocumentContentQuery { DocumentId = id }, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_ExistingExtractedContent_ReturnsTextAndTimestamp()
    {
        var id = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var extractedAt = DateTime.UtcNow;
        var document = new Document { Id = id, UserId = userId };
        var content = new DocumentContent
        {
            DocumentId = id,
            ExtractedText = "Extracted report text",
            ExtractedAt = extractedAt
        };
        _currentUser.SetupGet(x => x.UserId).Returns(userId);
        _documents.Setup(x => x.GetByIdAsync(id, userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(document);
        _content.Setup(x => x.GetByDocumentIdAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(content);

        var result = await CreateHandler().Handle(
            new GetDocumentContentQuery { DocumentId = id }, CancellationToken.None);

        Assert.Equal(id, result.DocumentId);
        Assert.Equal("Extracted report text", result.ExtractedText);
        Assert.Equal(extractedAt, result.ExtractedAt);
    }

    private GetDocumentContentQueryHandler CreateHandler() =>
        new(_content.Object, _documents.Object, _currentUser.Object);
}
