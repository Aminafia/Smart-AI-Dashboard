using Application.Common.Exceptions;
using Application.DTOs.AI;
using Application.Interfaces;
using Core.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AIController : ControllerBase
{
    private readonly IAIQueue _queue;
    private readonly IAIJobStore _jobStore;
    private readonly ILogger<AIController> _logger;

    public AIController(IAIQueue queue, IAIJobStore jobStore, ILogger<AIController> logger)
    {
        _queue = queue;
        _jobStore = jobStore;
        _logger = logger;
    }

    [Authorize]
    [EnableRateLimiting("fixed")]
    [HttpPost("generate")]
    public IActionResult Generate(AIRequest request)
    {
        _logger.LogInformation("[Controller] AI Generate endpoint hit");

        if (request == null || string.IsNullOrWhiteSpace(request.Prompt))
            throw new BadRequestException("Prompt cannot be empty");

        var job = new AIJob
        {
            ProjectId = Guid.NewGuid(),
            JobType = "Generate",
            Prompt = request.Prompt
        };

        _jobStore.Add(job);
        _queue.Enqueue(job);
        _logger.LogInformation("[Controller] AI job created with ID: {JobId}", job.Id);

        return Ok(new
        {
            jobId = job.Id,
            status = job.Status
        });
    }

    [Authorize]
    [HttpGet("status/{jobId}")]
    public IActionResult GetStatus(Guid jobId)
    {
        var job = _jobStore.Get(jobId);

        if (job is null)
            throw new NotFoundException("Job not found");

        return Ok(new
        {
            job.Id,
            job.Status,
            job.Result,
            job.Error
        });
    }
}