using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[ApiController]
[Route("api/ai")]
public class AIController : ControllerBase
{
    [HttpPost("analyze")]
    public IActionResult Analyze()
    {
        return Ok("TODO");
    }

    [HttpPost("summarize")]
    public IActionResult Summarize()
    {
        return Ok("TODO");
    }
}
