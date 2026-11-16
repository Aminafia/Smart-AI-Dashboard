using Application.Common.Exceptions;
using Application.Features.Users.Commands.CreateUser;
using Application.Interfaces;
using Core.Constants;
using Core.Entities;
using Moq;

namespace Application.Tests.Features.Users;

public class CreateUserCommandHandlerTests
{
    private readonly Mock<IUserRepository> _users = new();

    [Fact]
    public async Task Handle_NewEmail_CreatesUserWithHashedPasswordAndUserRole()
    {
        User? createdUser = null;
        var token = new CancellationTokenSource().Token;
        var command = new CreateUserCommand("amina@example.com", "Amina Afia", "secret-password");
        _users.Setup(x => x.EmailExistsAsync(command.Email, token)).ReturnsAsync(false);
        _users.Setup(x => x.AddUserAsync(It.IsAny<User>(), token))
            .Callback<User, CancellationToken>((user, _) => createdUser = user)
            .Returns(Task.CompletedTask);

        var result = await new CreateUserCommandHandler(_users.Object).Handle(command, token);

        Assert.NotEqual(Guid.Empty, result);
        Assert.NotNull(createdUser);
        Assert.Equal(result, createdUser.Id);
        Assert.Equal(command.Email, createdUser.Email);
        Assert.Equal(command.Name, createdUser.FullName);
        Assert.Equal(Roles.User, createdUser.Role);
        Assert.NotEqual(command.Password, createdUser.PasswordHash);
        Assert.True(BCrypt.Net.BCrypt.Verify(command.Password, createdUser.PasswordHash));
        Assert.Equal(DateTimeKind.Utc, createdUser.CreatedAt.Kind);
    }

    [Fact]
    public async Task Handle_ExistingEmail_ThrowsDuplicateEmailAndDoesNotCreateUser()
    {
        var command = new CreateUserCommand("existing@example.com", "Amina", "secret-password");
        _users.Setup(x => x.EmailExistsAsync(command.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        await Assert.ThrowsAsync<DuplicateEmailException>(
            () => new CreateUserCommandHandler(_users.Object).Handle(command, CancellationToken.None));

        _users.Verify(x => x.AddUserAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_NewEmail_PassesCancellationTokenToRepository()
    {
        var token = new CancellationTokenSource().Token;
        var command = new CreateUserCommand("amina@example.com", "Amina", "secret-password");
        _users.Setup(x => x.EmailExistsAsync(command.Email, token)).ReturnsAsync(false);
        _users.Setup(x => x.AddUserAsync(It.IsAny<User>(), token)).Returns(Task.CompletedTask);

        await new CreateUserCommandHandler(_users.Object).Handle(command, token);

        _users.Verify(x => x.EmailExistsAsync(command.Email, token), Times.Once);
        _users.Verify(x => x.AddUserAsync(It.IsAny<User>(), token), Times.Once);
    }
}
