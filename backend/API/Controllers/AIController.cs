using Application.Common.Exceptions;
using Application.DTOs.AI;
using Application.Interfaces;
using Core.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using MediatR;
using Application.Features.AI.Commands.GenerateAI;
using System.Threading.Tasks;
using Application.Features.AI.Queries.GetAIStatus;

namespace API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AIController : ControllerBase
{
    private readonly IAIQueue _queue;
    private readonly IAIJobStore _jobStore;
    private readonly ILogger<AIController> _logger;
    private readonly IMediator _mediator;
    public AIController(IAIQueue queue, IAIJobStore jobStore, ILogger<AIController> logger, IMediator mediator)
    {
        _queue = queue;
        _jobStore = jobStore;
        _logger = logger;
        _mediator = mediator;
    }

[Authorize]
[EnableRateLimiting("fixed")]
[HttpPost("generate")]
public async Task<IActionResult> Generate(AIRequest request)
{
    var result = await _mediator.Send(new GenerateAICommand
    {
        Prompt = request.Prompt
    });

    return Ok(result);
}

    [Authorize]
    [HttpGet("status/{jobId}")]
    public async Task<IActionResult> GetStatus(Guid jobId)
    {
        var result = await _mediator.Send(new GetAIStatusQuery
        {
            JobId = jobId 
        });

        return Ok(result);
    }
}