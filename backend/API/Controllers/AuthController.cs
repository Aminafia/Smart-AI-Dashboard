using Application.DTOs.Auth;
using Core.Interfaces;
using Microsoft.AspNetCore.Mvc;
using BCrypt.Net;
using Microsoft.AspNetCore.Authorization;
using Infrastructure.Auth;
using Application.Interfaces;

namespace API.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly IUserRepository _userRepository;
    private readonly IJwtTokenService _jwtTokenService;

    public AuthController(IUserRepository userRepository,
                          IJwtTokenService jwtTokenService) 
    {
        _userRepository = userRepository;
        _jwtTokenService = jwtTokenService;
    }

    [AllowAnonymous] 
    [HttpPost("login")]
    public async Task<ActionResult<AuthResponse>> Login(LoginRequest request)
    {
        var user = await _userRepository.GetByEmailAsync(request.Email);

        if (user == null ||
            !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
        {
            return Unauthorized("Invalid credentials");
        }

        var token = _jwtTokenService.GenerateToken(user);

        return Ok(new AuthResponse { Token = token });
    }
}
