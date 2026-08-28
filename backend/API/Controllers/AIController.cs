using Application.Common.Models;
using Application.DTOs.AI;
using Application.Features.AI.Commands.GenerateText;
using Application.Features.AI.Commands.Summarize;
using Application.Features.AI.Commands.SummarizeDocument;
using Application.Features.AI.Queries.GetAIJobs;
using Application.Features.AI.Queries.GetAIStatus;
using Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using MediatR;

namespace API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AIController : ControllerBase
{
    private readonly IMediator _mediator;

    public AIController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [Authorize]
    [EnableRateLimiting("fixed")]
    [HttpPost("generate")]
    public async Task<IActionResult> Generate(GenerateTextRequest request)
    {
        var result = await _mediator.Send(new GenerateTextCommand
        {
            Prompt = request.Prompt
        });

        return Ok(ApiResponse<AIOperationResponse>
            .SuccessResponse(result, "AI generation job created successfully."));
    }

    [Authorize]
    [HttpGet("status/{jobId}")]
    public async Task<IActionResult> GetStatus(Guid jobId)
    {
        var result = await _mediator.Send(new GetAIStatusQuery
        {
            JobId = jobId
        });

        return Ok(ApiResponse<AIStatusResponse>
            .SuccessResponse(result, "AI job status retrieved successfully."));
    }

    [Authorize]
    [HttpGet("jobs")]
    public async Task<IActionResult> GetJobs(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10)
    {
        var result = await _mediator.Send(new GetAIJobsQuery
        {
            Page = page,
            PageSize = pageSize
        });

        return Ok(ApiResponse<PagedResponse<GetAIJobsResponse>>
            .SuccessResponse(result, "AI jobs retrieved successfully."));
    }

    [Authorize]
    [HttpPost("summarize")]
    public async Task<IActionResult> Summarize([FromBody] SummarizeRequest request)
    {
        var result = await _mediator.Send(new SummarizeCommand
        {
            Text = request.Text
        });

        return Ok(ApiResponse<AIOperationResponse>
            .SuccessResponse(result, "Summary generated successfully."));
    }

    [Authorize]
    [HttpPost("documents/{documentId:guid}/summarize")]
    public async Task<IActionResult> SummarizeDocument(
        Guid documentId,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new SummarizeDocumentCommand
        {
            DocumentId = documentId
        }, cancellationToken);

        return Ok(ApiResponse<AIOperationResponse>
            .SuccessResponse(result, "Document summary generated successfully."));
    }
}
