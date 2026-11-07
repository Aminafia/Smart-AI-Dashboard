using Application.Common.Exceptions;
using Application.Common.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[ApiController]
[Route("api/test")]

/// To TEST your ExceptionHandlingMiddleware
/// You can add more endpoints here to test different scenarios (e.g. authentication, authorization, etc.)
public class TestController : ControllerBase
{
    [HttpGet("error")]
    public IActionResult ThrowError()
    { //deliberately throwing an exception to test the middleware
        throw new BadRequestException("Test exception for middleware"); 
    }

    [Authorize]
    [HttpGet("secure")]
    public IActionResult Secure()
    { //test authenticated endpoint to verify that authentication is working and not being blocked by the middleware
        return Ok(ApiResponse<string>.SuccessResponse("You are authenticated!", "Success")); 
    }

    [Authorize(Policy = "AdminOnly")]
    [HttpGet("admin")]
    public IActionResult AdminOnly()
    { //test admin-only endpoint to verify that authorization is working and not being blocked by the middleware
        return Ok(ApiResponse<string>.SuccessResponse("You are an admin!", "Success")); 
    }
}