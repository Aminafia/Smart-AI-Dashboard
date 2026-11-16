using Application.DTOs.AI;
using Application.Features.AI.Commands.Summarize;
using Application.Interfaces;
using Core.Entities;
using Core.Enums;
using Moq;

namespace Application.Tests.Features.AI;

public class SummarizeCommandHandlerTests
{
    private readonly Mock<IAIService> _aiService = new();
    private readonly Mock<IAIJobStore> _jobStore = new();
    private readonly Mock<ICurrentUserService> _currentUser = new();

    [Fact]
    public async Task Handle_Success_ReturnsSummaryAndMarksJobCompleted()
    {
        var userId = Guid.NewGuid();
        AIJob? savedJob = null;
        _currentUser.SetupGet(x => x.UserId).Returns(userId);
        _jobStore.Setup(x => x.AddJobAsync(It.IsAny<AIJob>()))
            .Callback<AIJob>(job => savedJob = job).Returns(Task.CompletedTask);
        _jobStore.Setup(x => x.UpdateJobAsync(It.IsAny<AIJob>())).Returns(Task.CompletedTask);
        _aiService.Setup(x => x.ProcessAsync(
                It.Is<AIRequest>(request => request.Input == "Long source text" && request.JobType == AIJobType.Summarize),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new AIProviderResponse { Output = "Concise summary", IsFallback = true });

        var result = await CreateHandler().Handle(
            new SummarizeCommand { Text = "Long source text" }, CancellationToken.None);

        Assert.NotNull(savedJob);
        Assert.Equal(userId, savedJob.UserId);
        Assert.Equal("Long source text", savedJob.Prompt);
        Assert.Equal(AIJobStatus.Completed, savedJob.Status);
        Assert.Equal("Concise summary", savedJob.Result);
        Assert.NotNull(savedJob.CompletedAt);
        Assert.Equal(savedJob.Id, result.JobId);
        Assert.Equal("Concise summary", result.Output);
        Assert.True(result.IsFallback);
        _jobStore.Verify(x => x.UpdateJobAsync(savedJob), Times.Once);
    }

    [Fact]
    public async Task Handle_AIProviderFails_MarksJobFailedAndRethrows()
    {
        AIJob? savedJob = null;
        _currentUser.SetupGet(x => x.UserId).Returns(Guid.NewGuid());
        _jobStore.Setup(x => x.AddJobAsync(It.IsAny<AIJob>()))
            .Callback<AIJob>(job => savedJob = job).Returns(Task.CompletedTask);
        _jobStore.Setup(x => x.UpdateJobAsync(It.IsAny<AIJob>())).Returns(Task.CompletedTask);
        _aiService.Setup(x => x.ProcessAsync(It.IsAny<AIRequest>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Provider unavailable"));

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            CreateHandler().Handle(new SummarizeCommand { Text = "Text to summarize" }, CancellationToken.None));

        Assert.NotNull(savedJob);
        Assert.Equal(AIJobStatus.Failed, savedJob.Status);
        Assert.Equal("Provider unavailable", savedJob.Error);
        Assert.NotNull(savedJob.CompletedAt);
        _jobStore.Verify(x => x.UpdateJobAsync(savedJob), Times.Once);
    }

    private SummarizeCommandHandler CreateHandler() =>
        new(_aiService.Object, _jobStore.Object, _currentUser.Object);
}
