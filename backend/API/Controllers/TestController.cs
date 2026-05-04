using Application.Common.Exceptions;
using Application.Common.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[ApiController]
[Route("api/test")]

/// <summary>
/// To TEST your ExceptionHandlingMiddleware
/// You can add more endpoints here to test different scenarios (e.g. authentication, authorization, etc.)
public class TestController : ControllerBase
{
    [HttpGet("error")]
    public IActionResult ThrowError()
    {
        throw new BadRequestException("Test exception for middleware"); //deliberately throwing an exception to test the middleware
    }

    [Authorize]
    [HttpGet("secure")]
    public IActionResult Secure()
    {
        return Ok(ApiResponse<string>.SuccessResponse("You are authenticated!", "Success")); //test authenticated endpoint to verify that authentication is working and not being blocked by the middleware
    }

    [Authorize(Policy = "AdminOnly")]
    [HttpGet("admin")]
    public IActionResult AdminOnly()
    {
        return Ok(ApiResponse<string>.SuccessResponse("You are an admin!", "Success")); //test admin-only endpoint to verify that authorization is working and not being blocked by the middleware
    }
}