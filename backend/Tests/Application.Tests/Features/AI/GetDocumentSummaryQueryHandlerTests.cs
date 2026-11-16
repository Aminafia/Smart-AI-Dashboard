using Application.Features.AI.Queries.GetDocumentSummary;
using Application.Interfaces;
using Core.Entities;
using Moq;

namespace Application.Tests.Features.AI;

public class GetDocumentSummaryQueryHandlerTests
{
    private readonly Mock<IAIJobStore> _jobs = new();
    private readonly Mock<ICurrentUserService> _currentUser = new();

    [Fact]
    public async Task Handle_NoCompletedSummary_ReturnsDocumentIdWithNullSummary()
    {
        var documentId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        _currentUser.SetupGet(x => x.UserId).Returns(userId);
        _jobs.Setup(x => x.GetLatestCompletedSummaryAsync(documentId, userId))
            .ReturnsAsync((AIJob?)null);

        var result = await CreateHandler().Handle(
            new GetDocumentSummaryQuery { DocumentId = documentId }, CancellationToken.None);

        Assert.Equal(documentId, result.DocumentId);
        Assert.Null(result.Summary);
        Assert.Null(result.GeneratedAt);
    }

    [Fact]
    public async Task Handle_CompletedSummaryExists_ReturnsSummaryAndGenerationTime()
    {
        var documentId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var completedAt = DateTime.UtcNow;
        _currentUser.SetupGet(x => x.UserId).Returns(userId);
        _jobs.Setup(x => x.GetLatestCompletedSummaryAsync(documentId, userId))
            .ReturnsAsync(new AIJob { DocumentId = documentId, Result = "Summary", CompletedAt = completedAt });

        var result = await CreateHandler().Handle(
            new GetDocumentSummaryQuery { DocumentId = documentId }, CancellationToken.None);

        Assert.Equal(documentId, result.DocumentId);
        Assert.Equal("Summary", result.Summary);
        Assert.Equal(completedAt, result.GeneratedAt);
    }

    private GetDocumentSummaryQueryHandler CreateHandler() =>
        new(_jobs.Object, _currentUser.Object);
}
