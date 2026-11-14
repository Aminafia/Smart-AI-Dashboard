using Application.Common.Models;
using Application.Features.AI.Queries.GetAIJobs;
using Application.Interfaces;

using Core.Entities;
using Core.Enums;

using Moq;

namespace Application.Tests.Features.AI;

public class GetAIJobsQueryHandlerTests
{
    private readonly Mock<IAIJobStore> _jobs = new();
    private readonly Mock<ICurrentUserService> _currentUser = new();

    [Fact]
    public async Task Handle_ReturnsMappedJobsAndPreservesPaginationForCurrentUser()
    {
        var userId = Guid.NewGuid();
        var job = new AIJob
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            JobType = AIJobType.GenerateText,
            Status = AIJobStatus.Processing,
            Prompt = "Generate a report",
            CreatedAt = DateTime.UtcNow
        };
        _currentUser.SetupGet(x => x.UserId).Returns(userId);
        _jobs.Setup(x => x.GetJobsAsync(2, 5, userId)).ReturnsAsync(new PagedResponse<AIJob>
        {
            Items = [job],
            Page = 2,
            PageSize = 5,
            TotalCount = 11
        });

        var result = await new GetAIJobsQueryHandler(_jobs.Object, _currentUser.Object)
            .Handle(new GetAIJobsQuery { Page = 2, PageSize = 5 }, CancellationToken.None);

        var mappedJob = Assert.Single(result.Items);
        Assert.Equal(job.Id, mappedJob.Id);
        Assert.Equal("GenerateText", mappedJob.JobType);
        Assert.Equal(AIJobStatus.Processing, mappedJob.Status);
        Assert.Equal(job.Prompt, mappedJob.Prompt);
        Assert.Equal(2, result.Page);
        Assert.Equal(5, result.PageSize);
        Assert.Equal(11, result.TotalCount);
        Assert.Equal(3, result.TotalPages);
        _jobs.Verify(x => x.GetJobsAsync(2, 5, userId), Times.Once);
    }
}
