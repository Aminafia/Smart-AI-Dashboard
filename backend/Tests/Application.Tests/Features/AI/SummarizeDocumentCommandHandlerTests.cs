using Application.Common.Exceptions;
using Application.DTOs.AI;
using Application.Features.AI.Commands.SummarizeDocument;
using Application.Interfaces;

using Core.Entities;
using Core.Enums;

using Moq;

namespace Application.Tests.Features.AI;

public class SummarizeDocumentCommandHandlerTests
{
    private readonly Mock<IDocumentContentRepository> _content = new();
    private readonly Mock<IDocumentRepository> _documents = new();
    private readonly Mock<IAIService> _aiService = new();
    private readonly Mock<IAIJobStore> _jobs = new();
    private readonly Mock<ICurrentUserService> _currentUser = new();

    [Fact]
    public async Task Handle_DocumentNotFound_ThrowsNotFoundWithoutReadingContent()
    {
        var documentId = Guid.NewGuid();
        _currentUser.SetupGet(x => x.UserId).Returns(Guid.NewGuid());
        _documents.Setup(x => x.GetByIdAsync(documentId, It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Document?)null);

        await Assert.ThrowsAsync<NotFoundException>(() =>
            CreateHandler().Handle(new SummarizeDocumentCommand { DocumentId = documentId }, CancellationToken.None));

        _content.Verify(x => x.GetByDocumentIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
        _aiService.Verify(x => x.ProcessAsync(It.IsAny<AIRequest>(), It.IsAny<CancellationToken>()), Times.Never);
        _jobs.Verify(x => x.AddJobAsync(It.IsAny<AIJob>()), Times.Never);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task Handle_MissingOrBlankExtractedText_ThrowsNotFound(string? extractedText)
    {
        var documentId = Guid.NewGuid();
        var document = new Document { Id = documentId, UserId = Guid.NewGuid() };
        _currentUser.SetupGet(x => x.UserId).Returns(document.UserId);
        _documents.Setup(x => x.GetByIdAsync(documentId, document.UserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(document);
        _content.Setup(x => x.GetByDocumentIdAsync(documentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(extractedText is null ? null : new DocumentContent { DocumentId = documentId, ExtractedText = extractedText });

        await Assert.ThrowsAsync<NotFoundException>(() =>
            CreateHandler().Handle(new SummarizeDocumentCommand { DocumentId = documentId }, CancellationToken.None));

        _jobs.Verify(x => x.AddJobAsync(It.IsAny<AIJob>()), Times.Never);
        _aiService.Verify(x => x.ProcessAsync(It.IsAny<AIRequest>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ValidExtractedText_CreatesDocumentLinkedSummaryJob()
    {
        var documentId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var document = new Document { Id = documentId, UserId = userId };
        AIJob? savedJob = null;
        _currentUser.SetupGet(x => x.UserId).Returns(userId);
        _documents.Setup(x => x.GetByIdAsync(documentId, userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(document);
        _content.Setup(x => x.GetByDocumentIdAsync(documentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new DocumentContent { DocumentId = documentId, ExtractedText = "Extracted report text" });
        _jobs.Setup(x => x.AddJobAsync(It.IsAny<AIJob>()))
            .Callback<AIJob>(job => savedJob = job).Returns(Task.CompletedTask);
        _jobs.Setup(x => x.UpdateJobAsync(It.IsAny<AIJob>())).Returns(Task.CompletedTask);
        _aiService.Setup(x => x.ProcessAsync(It.IsAny<AIRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new AIProviderResponse { Output = "Document summary" });

        var result = await CreateHandler().Handle(
            new SummarizeDocumentCommand { DocumentId = documentId }, CancellationToken.None);

        Assert.NotNull(savedJob);
        Assert.Equal(userId, savedJob.UserId);
        Assert.Equal(documentId, savedJob.DocumentId);
        Assert.Equal("Extracted report text", savedJob.Prompt);
        Assert.Equal(AIJobStatus.Completed, savedJob.Status);
        Assert.Equal("Document summary", result.Output);
    }

    private SummarizeDocumentCommandHandler CreateHandler() =>
        new(_content.Object, _documents.Object, _aiService.Object, _jobs.Object, _currentUser.Object);
}
