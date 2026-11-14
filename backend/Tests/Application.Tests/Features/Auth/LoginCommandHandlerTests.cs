using Application.Common.Exceptions;
using Application.Features.Auth.Commands.Login;
using Application.Interfaces;

using Core.Entities;

using Microsoft.Extensions.Logging;

using Moq;

namespace Application.Tests.Features.Auth;

public class LoginCommandHandlerTests
{
    private readonly Mock<IUserRepository> _users = new();
    private readonly Mock<IJwtTokenService> _tokens = new();
    private readonly Mock<ILogger<LoginCommandHandler>> _logger = new();

    private LoginCommandHandler CreateHandler() =>
        new(_users.Object, _tokens.Object, _logger.Object);

    [Fact]
    public async Task Handle_ValidCredentials_ReturnsTokenAndEmail()
    {
        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = "amina@example.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("correct-password")
        };
        var command = new LoginCommand { Email = user.Email, Password = "correct-password" };
        using var cancellation = new CancellationTokenSource();
        _users.Setup(x => x.GetUserByEmailAsync(command.Email, cancellation.Token)).ReturnsAsync(user);
        _tokens.Setup(x => x.GenerateToken(user)).Returns("jwt-token");

        var result = await CreateHandler().Handle(command, cancellation.Token);

        Assert.Equal("jwt-token", result.Token);
        Assert.Equal(user.Email, result.Email);
        _tokens.Verify(x => x.GenerateToken(user), Times.Once);
    }

    [Fact]
    public async Task Handle_UnknownEmail_ThrowsUnauthorizedAndDoesNotGenerateToken()
    {
        var command = new LoginCommand { Email = "missing@example.com", Password = "password" };
        _users.Setup(x => x.GetUserByEmailAsync(command.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        await Assert.ThrowsAsync<UnauthorizedException>(
            () => CreateHandler().Handle(command, CancellationToken.None));

        _tokens.Verify(x => x.GenerateToken(It.IsAny<User>()), Times.Never);
    }

    [Fact]
    public async Task Handle_IncorrectPassword_ThrowsUnauthorizedAndDoesNotGenerateToken()
    {
        var user = new User
        {
            Email = "amina@example.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("correct-password")
        };
        var command = new LoginCommand { Email = user.Email, Password = "incorrect-password" };
        _users.Setup(x => x.GetUserByEmailAsync(command.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        await Assert.ThrowsAsync<UnauthorizedException>(
            () => CreateHandler().Handle(command, CancellationToken.None));

        _tokens.Verify(x => x.GenerateToken(It.IsAny<User>()), Times.Never);
    }

    [Fact]
    public async Task Handle_PassesCancellationTokenToRepository()
    {
        var token = new CancellationTokenSource().Token;
        var command = new LoginCommand { Email = "amina@example.com", Password = "password" };
        _users.Setup(x => x.GetUserByEmailAsync(command.Email, token)).ReturnsAsync((User?)null);

        await Assert.ThrowsAsync<UnauthorizedException>(
            () => CreateHandler().Handle(command, token));

        _users.Verify(x => x.GetUserByEmailAsync(command.Email, token), Times.Once);
    }
}
