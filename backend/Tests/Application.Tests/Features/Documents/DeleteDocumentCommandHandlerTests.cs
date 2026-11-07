using Application.Common.Exceptions;
using Application.Features.Documents.Commands.DeleteDocument;
using Application.Interfaces;
using Core.Entities;
using Moq;

namespace Application.Tests.Features.Documents;

public class DeleteDocumentCommandHandlerTests
{
    private readonly Mock<IDocumentRepository> _documents = new();
    private readonly Mock<IDocumentStorage> _storage = new();
    private readonly Mock<ICurrentUserService> _currentUser = new();

    [Fact]
    public async Task Handle_DocumentNotOwnedOrMissing_ThrowsNotFoundAndDoesNotDeleteStorage()
    {
        var id = Guid.NewGuid();
        _currentUser.SetupGet(x => x.UserId).Returns(Guid.NewGuid());
        _documents.Setup(x => x.GetByIdAsync(id, It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Document?)null);

        await Assert.ThrowsAsync<NotFoundException>(() =>
            CreateHandler().Handle(new DeleteDocumentCommand { Id = id }, CancellationToken.None));

        _storage.Verify(x => x.DeleteAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
        _documents.Verify(x => x.DeleteAsync(It.IsAny<Document>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ExistingDocument_DeletesPhysicalFileBeforeDatabaseRecord()
    {
        var id = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var document = new Document { Id = id, UserId = userId, StoragePath = "storage/report.pdf" };
        var token = new CancellationTokenSource().Token;
        _currentUser.SetupGet(x => x.UserId).Returns(userId);
        _documents.Setup(x => x.GetByIdAsync(id, userId, token)).ReturnsAsync(document);
        var sequence = new MockSequence();
        _storage.InSequence(sequence).Setup(x => x.DeleteAsync(document.StoragePath, token))
            .Returns(Task.CompletedTask);
        _documents.InSequence(sequence).Setup(x => x.DeleteAsync(document, token))
            .Returns(Task.CompletedTask);

        await CreateHandler().Handle(new DeleteDocumentCommand { Id = id }, token);

        _storage.Verify(x => x.DeleteAsync(document.StoragePath, token), Times.Once);
        _documents.Verify(x => x.DeleteAsync(document, token), Times.Once);
    }

    [Fact]
    public async Task Handle_StorageDeleteFails_KeepsDatabaseRecord()
    {
        var id = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var document = new Document { Id = id, UserId = userId, StoragePath = "storage/report.pdf" };
        _currentUser.SetupGet(x => x.UserId).Returns(userId);
        _documents.Setup(x => x.GetByIdAsync(id, userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(document);
        _storage.Setup(x => x.DeleteAsync(document.StoragePath, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new IOException("Storage unavailable"));

        await Assert.ThrowsAsync<IOException>(() =>
            CreateHandler().Handle(new DeleteDocumentCommand { Id = id }, CancellationToken.None));

        _documents.Verify(x => x.DeleteAsync(It.IsAny<Document>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    private DeleteDocumentCommandHandler CreateHandler() =>
        new(_documents.Object, _storage.Object, _currentUser.Object);
}
