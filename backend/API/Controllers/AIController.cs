using Application.DTOs.AI;
using Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AIController : ControllerBase
{
    private readonly IAIService _aiService;

    public AIController(IAIService aiService)
    {
        _aiService = aiService;
    }
    
    [Authorize]
    [HttpPost("generate")]
    public async Task<ActionResult<AIResponse>> Generate(AIRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Prompt))
        {
            return BadRequest("Prompt cannot be empty");
        }

        var response = await _aiService.GenerateAsync(request);
        return Ok(response);
    }
}