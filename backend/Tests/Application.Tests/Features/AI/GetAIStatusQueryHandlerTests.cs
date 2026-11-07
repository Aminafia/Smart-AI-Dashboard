using Application.Common.Exceptions;
using Application.Features.AI.Queries.GetAIStatus;
using Application.Interfaces;

using Core.Entities;
using Core.Enums;

using Moq;

namespace Application.Tests.Features.AI;

public class GetAIStatusQueryHandlerTests
{
    private readonly Mock<IAIJobStore> _jobs = new();
    private readonly Mock<ICurrentUserService> _currentUser = new();

    [Fact]
    public async Task Handle_JobExists_ReturnsStatusForCurrentUser()
    {
        var jobId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var createdAt = DateTime.UtcNow.AddMinutes(-2);
        var completedAt = DateTime.UtcNow;
        var job = new AIJob
        {
            Id = jobId,
            UserId = userId,
            JobType = AIJobType.Summarize,
            Prompt = "Source text",
            Status = AIJobStatus.Completed,
            Result = "Summary",
            CreatedAt = createdAt,
            CompletedAt = completedAt
        };
        _currentUser.SetupGet(x => x.UserId).Returns(userId);
        _jobs.Setup(x => x.GetJobAsync(jobId, userId)).ReturnsAsync(job);

        var result = await CreateHandler().Handle(new GetAIStatusQuery { JobId = jobId }, CancellationToken.None);

        Assert.Equal(jobId, result.Id);
        Assert.Equal("Summarize", result.JobType);
        Assert.Equal("Source text", result.Prompt);
        Assert.Equal(AIJobStatus.Completed, result.Status);
        Assert.Equal("Summary", result.Result);
        Assert.Equal(createdAt, result.CreatedAt);
        Assert.Equal(completedAt, result.CompletedAt);
        _jobs.Verify(x => x.GetJobAsync(jobId, userId), Times.Once);
    }

    [Fact]
    public async Task Handle_JobNotFoundForCurrentUser_ThrowsNotFound()
    {
        var jobId = Guid.NewGuid();
        _currentUser.SetupGet(x => x.UserId).Returns(Guid.NewGuid());
        _jobs.Setup(x => x.GetJobAsync(jobId, It.IsAny<Guid>())).ReturnsAsync((AIJob?)null);

        await Assert.ThrowsAsync<NotFoundException>(() =>
            CreateHandler().Handle(new GetAIStatusQuery { JobId = jobId }, CancellationToken.None));
    }

    private GetAIStatusQueryHandler CreateHandler() =>
        new(_jobs.Object, _currentUser.Object);
}
