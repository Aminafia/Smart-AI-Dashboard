using Application.DTOs.Auth;
using Application.Services;
using Core.Interfaces;
using Microsoft.AspNetCore.Mvc;
using BCrypt.Net;
using Microsoft.AspNetCore.Authorization;

namespace API.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly IUserRepository _userRepository;
    private readonly JwtTokenService _jwtTokenService;

    public AuthController(IUserRepository userRepository,
                          JwtTokenService jwtTokenService) 
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
