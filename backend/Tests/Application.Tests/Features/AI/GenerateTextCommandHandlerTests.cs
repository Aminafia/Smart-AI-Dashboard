using Application.Features.AI.Commands.GenerateText;
using Application.Interfaces;

using Core.Entities;
using Core.Enums;

using Microsoft.Extensions.Logging;

using Moq;

namespace Application.Tests.Features.AI;

public class GenerateTextCommandHandlerTests
{
    private readonly Mock<IAIQueue> _queue = new();
    private readonly Mock<IAIJobStore> _jobStore = new();
    private readonly Mock<ICurrentUserService> _currentUser = new();
    private readonly Mock<ILogger<GenerateTextCommandHandler>> _logger = new();

    [Fact]
    public async Task Handle_ValidPrompt_PersistsAndEnqueuesPendingJobForCurrentUser()
    {
        var userId = Guid.NewGuid();
        AIJob? savedJob = null;
        AIJob? queuedJob = null;
        _currentUser.SetupGet(x => x.UserId).Returns(userId);
        _jobStore.Setup(x => x.AddJobAsync(It.IsAny<AIJob>()))
            .Callback<AIJob>(job => savedJob = job)
            .Returns(Task.CompletedTask);
        _queue.Setup(x => x.Enqueue(It.IsAny<AIJob>()))
            .Callback<AIJob>(job => queuedJob = job);

        var result = await CreateHandler().Handle(
            new GenerateTextCommand { Prompt = "Explain retrieval augmented generation" },
            CancellationToken.None);

        Assert.NotNull(savedJob);
        Assert.Same(savedJob, queuedJob);
        Assert.Equal(userId, savedJob.UserId);
        Assert.Equal("Explain retrieval augmented generation", savedJob.Prompt);
        Assert.Equal(AIJobType.GenerateText, savedJob.JobType);
        Assert.Equal(AIJobStatus.Pending, savedJob.Status);
        Assert.NotEqual(Guid.Empty, savedJob.ProjectId);
        Assert.Equal(savedJob.Id, result.JobId);
        Assert.Equal(AIJobStatus.Pending, result.Status);
        Assert.Equal(string.Empty, result.Output);
        Assert.False(result.IsFallback);
    }

    [Fact]
    public async Task Handle_PersistenceFails_DoesNotEnqueueJob()
    {
        _currentUser.SetupGet(x => x.UserId).Returns(Guid.NewGuid());
        _jobStore.Setup(x => x.AddJobAsync(It.IsAny<AIJob>()))
            .ThrowsAsync(new InvalidOperationException("Database unavailable"));

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            CreateHandler().Handle(new GenerateTextCommand { Prompt = "prompt" }, CancellationToken.None));

        _queue.Verify(x => x.Enqueue(It.IsAny<AIJob>()), Times.Never);
    }

    private GenerateTextCommandHandler CreateHandler() =>
        new(_queue.Object, _jobStore.Object, _currentUser.Object, _logger.Object);
}
