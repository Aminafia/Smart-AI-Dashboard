using MediatR;
using Core.Entities;
using Core.Interfaces;
using Infrastructure.Auth;
using Application.Services;


namespace Application.Features.Auth.Commands.Login;

public class LoginCommandHandler
    : IRequestHandler<LoginCommand, string>
{
    private readonly IUserRepository _userRepository;
    private readonly JwtTokenService _jwtTokenService;

    public LoginCommandHandler(
        IUserRepository userRepository,
        JwtTokenService jwtTokenService)
    {
        _userRepository = userRepository;
        _jwtTokenService = jwtTokenService;
    }

    public async Task<string> Handle(
        LoginCommand request,
        CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByEmailAsync(request.Email);

        if (user is null)
            throw new Exception("Invalid email or password");

        var isPasswordValid = BCrypt.Net.BCrypt.Verify(
            request.Password,
            user.PasswordHash);

        if (!isPasswordValid)
            throw new Exception("Invalid email or password");

        return _jwtTokenService.GenerateToken(user);
    }
}
