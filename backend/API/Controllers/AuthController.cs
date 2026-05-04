/*
User logs in with Eamil and password 
    |
    | LoginRequest(Email, Password)
    ↓ 
(----API----)
AuthController - converts LoginRequest to LoginCommand via MediatR
    |
    |LoginCommand(Email, Password)
    ↓
MediatR - routes LoginCommand to LoginCommandHandler
    |
    |LoginCommand(Email, Password)
    ↓
(---Application---)
LoginCommandHandler - processes the command using IUserRepository to fetch user data (still in Application layer), validates credentials, generates JWT token
    |
    | LoginResponse(Token, Email)
    ↓ 
IUserRepository (Application)
    |
    | call UserRepository.GetByEmailAsync(Email)
    ↓
(---Infrastructure---)
UserRepository (Infrastructure)
    |
    | accesses Database using AppDbContext to fetch User entity from Users table in Db
    ↓
(---Database---)
DbContext - executes query to get user from database and maps result to User entity
    | User Entity in Core
    ↓
--Application---
LoginCommandHandler - receives User entity, verifies password, generates JWT token
    | LoginResponse(Token, Email)
    ↓
AuthController wraps LoginResponse in ApiResponse
    |
    | ApiResponse<LoginResponse>(Token, "Login successful")
    ↓
Client - receives JWT token for authenticated access to protected resources
*/
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
    /// 1. Receives `LoginRequest` from client as JSON payload containing Email and Password.
    /// 2. Converts it into a command `LoginCommand`
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