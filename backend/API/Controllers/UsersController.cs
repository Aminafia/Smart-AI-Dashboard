using Application.Common.Models;
using Application.Features.Users.Commands.CreateUser;
using Application.Features.Users.Queries.GetUsers;
using Application.Features.Users.DTOs;
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

    public UsersController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost]
    public async Task<IActionResult> CreateUser([FromBody] CreateUserCommand command)
    {
        var result = await _mediator.Send(command);

        return Ok(ApiResponse<Guid>.SuccessResponse(result, "User created successfully"));
    }

    [HttpGet]
    public async Task<IActionResult> GetUsers()
    {
        var result = await _mediator.Send(new GetUsersQuery());

        return Ok(ApiResponse<List<UserDto>>.SuccessResponse(result, "Users fetched successfully"));
    }
}