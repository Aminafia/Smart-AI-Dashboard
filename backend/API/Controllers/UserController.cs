using Application.DTOs.Users;
using Application.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private readonly IUserService _userService;

    public UsersController(IUserService userService)
    {
        _userService = userService;
    }

    [HttpPost]
    public IActionResult CreateUser([FromBody] CreateUserRequest request)
    {
        var result = _userService.CreateUser(request);
        return Ok(result);
    }

    [HttpGet]
    public IActionResult GetUsers()
    {
        return Ok(_userService.GetAllUsers());
    }
}