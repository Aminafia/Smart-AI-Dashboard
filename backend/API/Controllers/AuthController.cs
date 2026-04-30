using Application.Common.Models;
using Application.DTOs.Auth;
using Application.Features.Auth.Commands.Login;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Serilog;

namespace API.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly IMediator _mediator;

    public AuthController(IMediator mediator)
    {
        _mediator = mediator;
    }


    /// <summary>
    /// Handles authentication endpoints.
    /// 1. Receives `LoginRequest` from client
    /// 2. Converts it into a command `LoginCommand` using Email and Password
    /// 3. Controller delegates control (sends the `LoginCommand`) to MediatR- that finds the correct handler.
    /// 4. Receives `LoginResponse` containing JWT token from the handler.
    /// 5. Returns the token wrapped in a standardized `ApiResponse` to the client.
    /// </summary>
    [AllowAnonymous]
    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginRequest request)
    {
        Log.Information("[Controller] Login endpoint hit");
        var command = new LoginCommand
        {
            Email = request.Email,
            Password = request.Password
        };

        var result = await _mediator.Send(command);

        Log.Information("[Controller] Login successful, returning response");

        return Ok(ApiResponse<LoginResponse>.SuccessResponse(result, "Login successful"));
    }
}