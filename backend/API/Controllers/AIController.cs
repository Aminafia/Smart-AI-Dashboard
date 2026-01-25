using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Core.Constants;
using Core.AI.Interfaces;
using Core.AI.Models;
using Core.AI.Services;

namespace API.Controllers;

[ApiController]
[Route("api/ai")]
[Authorize(Roles = Roles.User)]
public class AIController : ControllerBase
{

    private readonly IAIService _aiService;

    public AIController(IAIService aiService)
    {
        _aiService = aiService;
    }
    
    [HttpPost("analyze")]
    public async Task<IActionResult> Analyze([FromBody] AIRequest request)
    {
        var response = await _aiService.AnalyzeAsync(request);
        return Ok(response);
    }

    [HttpPost("summarize")]
    public IActionResult Summarize()
    {
        return Ok("TODO");
    }
}
