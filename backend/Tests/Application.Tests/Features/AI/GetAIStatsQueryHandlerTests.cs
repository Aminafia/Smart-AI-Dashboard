using Application.Features.AI.Queries.GetAIStats;
using Application.Interfaces;

using Moq;

namespace Application.Tests.Features.AI;

public class GetAIStatsQueryHandlerTests
{
    [Fact]
    public async Task Handle_ReturnsStatisticsForCurrentUser()
    {
        var userId = Guid.NewGuid();
        var expected = new AIStatsResponse
        {
            TotalJobs = 15,
            CompletedJobs = 10,
            ProcessingJobs = 3,
            FailedJobs = 2
        };
        var jobs = new Mock<IAIJobStore>();
        var currentUser = new Mock<ICurrentUserService>();
        currentUser.SetupGet(x => x.UserId).Returns(userId);
        jobs.Setup(x => x.GetStatsAsync(userId)).ReturnsAsync(expected);

        var result = await new GetAIStatsQueryHandler(jobs.Object, currentUser.Object)
            .Handle(new GetAIStatsQuery(), CancellationToken.None);

        Assert.Same(expected, result);
        jobs.Verify(x => x.GetStatsAsync(userId), Times.Once);
    }
}
