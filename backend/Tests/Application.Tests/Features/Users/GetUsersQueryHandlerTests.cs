using Application.Features.Users.Queries.GetUsers;
using Application.Interfaces;

using Core.Entities;

using Moq;

namespace Application.Tests.Features.Users;

public class GetUsersQueryHandlerTests
{
    [Fact]
    public async Task Handle_UsersExist_ReturnsMappedUserDtos()
    {
        var users = new List<User>
        {
            new() { Id = Guid.NewGuid(), Email = "one@example.com", FullName = "User One", Role = "User" },
            new() { Id = Guid.NewGuid(), Email = "two@example.com", FullName = "User Two", Role = "Admin" }
        };
        var repository = new Mock<IUserRepository>();
        repository.Setup(x => x.GetAllUsersAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(users);

        var result = await new GetUsersQueryHandler(repository.Object)
            .Handle(new GetUsersQuery(), CancellationToken.None);

        Assert.Equal(2, result.Count);
        Assert.Equal(users[0].Id, result[0].Id);
        Assert.Equal(users[0].Email, result[0].Email);
        Assert.Equal(users[0].FullName, result[0].FullName);
        Assert.Equal(users[0].Role, result[0].Role);
        Assert.Equal(users[1].Id, result[1].Id);
    }

    [Fact]
    public async Task Handle_NoUsers_ReturnsEmptyList()
    {
        var repository = new Mock<IUserRepository>();
        repository.Setup(x => x.GetAllUsersAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);

        var result = await new GetUsersQueryHandler(repository.Object)
            .Handle(new GetUsersQuery(), CancellationToken.None);

        Assert.Empty(result);
    }
}
