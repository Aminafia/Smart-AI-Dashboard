using MediatR;
using Core.Entities;
using Core.Interfaces;
using Application.Interfaces;
using Application.Features.Auth.Commands.Login;

namespace Application.Features.Auth.Commands.Login;

public class LoginCommandHandler
    : IRequestHandler<LoginCommand, LoginResponse>
{
    private readonly IUserRepository _userRepository;
    private readonly IJwtTokenService _jwtTokenService;

    public LoginCommandHandler(
        IUserRepository userRepository,
        IJwtTokenService jwtTokenService)
    {
        _userRepository = userRepository;
        _jwtTokenService = jwtTokenService;
    }

    public async Task<LoginResponse> Handle(
        LoginCommand request,
        CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByEmailAsync(request.Email);

        if (user is null)
            throw new UnauthorizedAccessException("Invalid email or password");

        var isPasswordValid = BCrypt.Net.BCrypt.Verify(
            request.Password,
            user.PasswordHash);

        if (!isPasswordValid)
            throw new UnauthorizedAccessException("Invalid email or password");

        return new LoginResponse
        {
            Token = _jwtTokenService.GenerateToken(user),
            Email = user.Email
        };
    }
}
