using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace API.Controllers;

[ApiController]
[Route("api/ai")]
[Authorize]
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
