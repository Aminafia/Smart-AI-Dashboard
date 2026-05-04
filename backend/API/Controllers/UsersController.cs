using Application.Common.Models;
using Application.Features.Users.Commands.CreateUser;
using Application.Features.Users.DTOs;
using Application.Features.Users.Queries.GetUsers;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[ApiController]
[Route("api/users")]
[Authorize(Policy = "AdminOnly")]
public class UsersController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<UsersController> _logger;

    public UsersController(IMediator mediator, ILogger<UsersController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    [HttpPost]
    public async Task<IActionResult> CreateUser([FromBody] CreateUserCommand command)
    {
        _logger.LogInformation("[Controller] CreateUser endpoint hit");

        var result = await _mediator.Send(command);

        _logger.LogInformation("[Controller] User created with ID: {UserId}", result);

        return Ok(ApiResponse<Guid>.SuccessResponse(result, "User created successfully"));
    }

    [HttpGet]
    public async Task<IActionResult> GetUsers()
    {
        _logger.LogInformation("[Controller] GetUsers endpoint hit");

        var result = await _mediator.Send(new GetUsersQuery());

        _logger.LogInformation("[Controller] Users fetched");

        return Ok(ApiResponse<List<UserDto>>.SuccessResponse(result, "Users fetched successfully"));
    }
}