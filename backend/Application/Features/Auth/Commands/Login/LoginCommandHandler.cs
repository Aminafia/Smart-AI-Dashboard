/*
Receive command
    ↓
    Fetch user from DB
    ↓
    Verify password
    ↓
    Generate JWT token
    ↓
Return response
*/

using Application.Common.Exceptions;
using Application.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Features.Auth.Commands.Login;

/// <summary>
/// Handles login authentication logic.
/// - Fetches user from database
/// - Verifies password using BCrypt
/// - Generates JWT token if valid
/// </summary>
public class LoginCommandHandler
    : IRequestHandler<LoginCommand, LoginResponse>
{
    private readonly IUserRepository _userRepository;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly ILogger<LoginCommandHandler> _logger;

    public LoginCommandHandler(
        IUserRepository userRepository,
        IJwtTokenService jwtTokenService,
        ILogger<LoginCommandHandler> logger)
    {
        _userRepository = userRepository;
        _jwtTokenService = jwtTokenService;
        _logger = logger;
    }

    /// <summary>
    /// Handles the login logic:
    /// 1. Receives `LoginCommand` request with email and password.
    /// 2. Fetches user from database using email.
    /// 3. Verifies password using BCrypt.
    /// 4. If valid, generates JWT token and returns it in `LoginResponse`.
    /// </summary>
    /// <param name="request"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    /// <exception cref="UnauthorizedException"></exception>
    public async Task<LoginResponse> Handle(
        LoginCommand request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("[MediatR] Handling LoginCommand");

        _logger.LogInformation("[Handler] Fetching user from DB");
        var user = await _userRepository.GetUserByEmailAsync(request.Email, cancellationToken);
        if (user is null)
        {
            _logger.LogWarning("[Handler] User not found");
            throw new UnauthorizedException("Invalid email or password");
        }

        _logger.LogInformation("[Handler] Verifying password");

        var isPasswordValid = BCrypt.Net.BCrypt.Verify(
            request.Password,
            user.PasswordHash);
        if (!isPasswordValid)
        {
            _logger.LogWarning("[Handler] Invalid password");
            throw new UnauthorizedException("Invalid email or password");
        }

        _logger.LogInformation("[Handler] Generating JWT token");
        return new LoginResponse
        {
            Token = _jwtTokenService.GenerateToken(user),
            Email = user.Email
        };
    }
}