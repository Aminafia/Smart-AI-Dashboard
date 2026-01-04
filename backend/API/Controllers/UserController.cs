using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[ApiController]
[Route("api/users")]
public class UsersController : ControllerBase
{
    [HttpPost]
    public IActionResult CreateUser()
    {
        return Ok("TODO");
    }

    [HttpGet("{id}")]
    public IActionResult GetUser(Guid id)
    {
        return Ok("TODO");
    }
}