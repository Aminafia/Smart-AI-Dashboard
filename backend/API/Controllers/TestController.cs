using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace API.Controllers;

[ApiController]
[Route("api/test")]
public class TestController : ControllerBase
{
    [HttpGet("error")]
    public IActionResult ThrowError()
    {
        throw new Exception("Test exception for middleware");
    }

    // Authenticated users only
    [Authorize]
    [HttpGet("secure")]
    public IActionResult Secure()
    {
        return Ok("You are authenticated!");
    }

    // Admins only
    [Authorize(Policy = "AdminOnly")]
    [HttpGet("admin")]
    public IActionResult AdminOnly()
    {
        return Ok("You are an admin!");
    }
}