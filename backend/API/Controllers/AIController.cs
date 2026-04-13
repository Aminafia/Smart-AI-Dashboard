using Application.Common.Exceptions;
using Application.Common.Models;
using Application.DTOs.AI;
using Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

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
    [EnableRateLimiting("fixed")]
    [HttpPost("generate")]
    public async Task<IActionResult> Generate(AIRequest request)
    {
        if (request == null || string.IsNullOrWhiteSpace(request.Prompt))
            throw new BadRequestException("Prompt cannot be empty");

        var response = await _aiService.GenerateAsync(request);

        return Ok(ApiResponse<AIResponse>.SuccessResponse(response, "AI response generated"));
    }
}