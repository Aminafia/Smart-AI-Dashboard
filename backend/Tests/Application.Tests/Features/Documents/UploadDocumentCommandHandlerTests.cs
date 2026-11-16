using Application.DTOs.Documents;
using Application.Features.Documents.Commands.UploadDocument;
using Application.Interfaces;
using Core.Entities;
using Moq;

namespace Application.Tests.Features.Documents;

public class UploadDocumentCommandHandlerTests
{
    private readonly Mock<IDocumentStorage> _storage = new();
    private readonly Mock<IDocumentRepository> _documents = new();
    private readonly Mock<ICurrentUserService> _currentUser = new();

    [Fact]
    public async Task Handle_ValidUpload_SavesFileAndPersistsDocumentMetadata()
    {
        var userId = Guid.NewGuid();
        using var stream = new MemoryStream([1, 2, 3, 4]);
        var upload = new DocumentUpload
        {
            FileName = "report.pdf",
            ContentType = "application/pdf",
            Content = stream
        };
        Document? saved = null;
        _currentUser.SetupGet(x => x.UserId).Returns(userId);
        _storage.Setup(x => x.SaveAsync(upload, It.IsAny<CancellationToken>()))
            .ReturnsAsync("stored/report.pdf");
        _documents.Setup(x => x.AddAsync(It.IsAny<Document>(), It.IsAny<CancellationToken>()))
            .Callback<Document, CancellationToken>((document, _) => saved = document)
            .Returns(Task.CompletedTask);

        var result = await CreateHandler().Handle(
            new UploadDocumentCommand { Document = upload }, CancellationToken.None);

        Assert.NotNull(saved);
        Assert.Equal(userId, saved.UserId);
        Assert.Equal("report.pdf", saved.FileName);
        Assert.Equal("stored/report.pdf", saved.StoredFileName);
        Assert.Equal("stored/report.pdf", saved.StoragePath);
        Assert.Equal("application/pdf", saved.ContentType);
        Assert.Equal(4, saved.FileSize);
        Assert.Equal(saved.Id, result.Id);
        Assert.Equal(saved.FileName, result.FileName);
        Assert.Equal(saved.ContentType, result.ContentType);
        Assert.Equal(saved.FileSize, result.FileSize);
    }

    [Fact]
    public async Task Handle_DatabaseSaveFails_DeletesStoredFileAndRethrows()
    {
        using var stream = new MemoryStream([1, 2, 3]);
        var upload = new DocumentUpload
        {
            FileName = "report.pdf",
            ContentType = "application/pdf",
            Content = stream
        };
        _currentUser.SetupGet(x => x.UserId).Returns(Guid.NewGuid());
        _storage.Setup(x => x.SaveAsync(upload, It.IsAny<CancellationToken>()))
            .ReturnsAsync("stored/report.pdf");
        _documents.Setup(x => x.AddAsync(It.IsAny<Document>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Database unavailable"));
        _storage.Setup(x => x.DeleteAsync("stored/report.pdf", CancellationToken.None))
            .Returns(Task.CompletedTask);

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            CreateHandler().Handle(new UploadDocumentCommand { Document = upload }, CancellationToken.None));

        _storage.Verify(x => x.DeleteAsync("stored/report.pdf", CancellationToken.None), Times.Once);
    }

    [Fact]
    public async Task Handle_StorageSaveFails_DoesNotCreateDatabaseRecord()
    {
        using var stream = new MemoryStream([1]);
        var upload = new DocumentUpload
        {
            FileName = "report.pdf",
            ContentType = "application/pdf",
            Content = stream
        };
        _storage.Setup(x => x.SaveAsync(upload, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new IOException("Disk unavailable"));

        await Assert.ThrowsAsync<IOException>(() =>
            CreateHandler().Handle(new UploadDocumentCommand { Document = upload }, CancellationToken.None));

        _documents.Verify(x => x.AddAsync(It.IsAny<Document>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    private UploadDocumentCommandHandler CreateHandler() =>
        new(_storage.Object, _documents.Object, _currentUser.Object);
}
