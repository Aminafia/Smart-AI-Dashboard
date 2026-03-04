using Microsoft.AspNetCore.Mvc;

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
}