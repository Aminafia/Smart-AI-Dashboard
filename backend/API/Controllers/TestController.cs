using Application.Common.Exceptions;
using Application.Common.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[ApiController]
[Route("api/test")]
public class TestController : ControllerBase
{
    [HttpGet("error")]
    public IActionResult ThrowError()
    {
        throw new BadRequestException("Test exception for middleware");
    }

    [Authorize]
    [HttpGet("secure")]
    public IActionResult Secure()
    {
        return Ok(ApiResponse<string>.SuccessResponse("You are authenticated!", "Success"));
    }

    [Authorize(Policy = "AdminOnly")]
    [HttpGet("admin")]
    public IActionResult AdminOnly()
    {
        return Ok(ApiResponse<string>.SuccessResponse("You are an admin!", "Success"));
    }
}