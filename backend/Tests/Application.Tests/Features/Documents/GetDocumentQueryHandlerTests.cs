using Application.Common.Exceptions;
using Application.Features.Documents.Queries.GetDocument;
using Application.Interfaces;

using Core.Entities;

using Moq;

namespace Application.Tests.Features.Documents;

public class GetDocumentQueryHandlerTests
{
    private readonly Mock<IDocumentRepository> _documents = new();
    private readonly Mock<IDocumentStorage> _storage = new();
    private readonly Mock<ICurrentUserService> _currentUser = new();

    [Fact]
    public async Task Handle_DocumentNotFound_ThrowsNotFoundWithoutOpeningFile()
    {
        var id = Guid.NewGuid();
        _currentUser.SetupGet(x => x.UserId).Returns(Guid.NewGuid());
        _documents.Setup(x => x.GetByIdAsync(id, It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Document?)null);

        await Assert.ThrowsAsync<NotFoundException>(() =>
            CreateHandler().Handle(new GetDocumentQuery { Id = id }, CancellationToken.None));

        _storage.Verify(x => x.OpenReadAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ExistingDocument_ReturnsFileStreamAndMetadata()
    {
        var id = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var document = new Document
        {
            Id = id,
            UserId = userId,
            FileName = "report.pdf",
            ContentType = "application/pdf",
            StoragePath = "stored/report.pdf"
        };
        var stream = new MemoryStream([1, 2, 3]);
        _currentUser.SetupGet(x => x.UserId).Returns(userId);
        _documents.Setup(x => x.GetByIdAsync(id, userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(document);
        _storage.Setup(x => x.OpenReadAsync(document.StoragePath, It.IsAny<CancellationToken>()))
            .ReturnsAsync(stream);

        var result = await CreateHandler().Handle(new GetDocumentQuery { Id = id }, CancellationToken.None);

        Assert.Same(stream, result.Content);
        Assert.Equal("report.pdf", result.FileName);
        Assert.Equal("application/pdf", result.ContentType);
    }

    private GetDocumentQueryHandler CreateHandler() =>
        new(_documents.Object, _storage.Object, _currentUser.Object);
}
