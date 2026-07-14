/*
-#-#-#-#-#-#-#-#-#-#-#-#-#-#-#-#-#-#-#-#-#-#-#-#-#-#-#-#-#-#-#-#-#-#-#-#-#-#-#-#-#-#-#-#-#-#-#-#-#-#-#-#-#-#
1. Generete endpoint:
   User
    |
    |
AIRequest(Prompt)
    |
    | 
    ↓ 
(---API-----------------------------)
AIController.Generate(AIRequest) - converts AIRequest to GenerateAICommand via MediatR
    |
    | GenerateAICommand(Prompt)
    ↓
MediatR - routes GenerateAICommand to GenerateAICommandHandler
    |
    | GenerateAICommand(Prompt)
    ↓
(---Application---------------------)
GenerateAICommandHandler.Handle(GenerateAICommand) - creates new AIJob with status "Pending",
    |            |                                              - adds this AIJob to IAIJobStore,
    |            |                                              - enqueues job in IAIQueue for processing                                                   - enqueues job in IAIQueue for processing
    |            | AIJob(Id, ProjectId, JobType, Prompt, Status="Pending", RetryCount=0, MaxRetries=3, CreatedAt)
    |            ↓
    |        IAIJobStore.AddJobAsync(AIJob)
    |            |
    |            | call AIJobStore.AddJobAsync(AIJob)
    |            ↓
    |        (---Infrastructure-------------------)
    |        AIJobStore.AddJobAsync(AIJob) - implements IAIJobStore, uses AppDbContext to save AIJob to database
    |            |
    |            | call AppDbContext.AddAsync(AIJob)
    |            ↓
    |        (---Database--------------------------)
    |        AppDbContext.AddAsync(AIJob) - executes query to insert new AIJob into database and maps result to AIJob entity
    |
    | IAIQueue.Enqueue(AIJob)
    |
    ↓
(---Infrastructure-------------------)
AIQueue.Enqueue(AIJob) - implements IAIQueue, adds AIJob to in-memory queue for processing by AIWorker
    |
    |
    ↓ 
In memory queue - AIJob waits until AIWorker picks it up for processing
    |            |                                              -  returns GenerateAIResponse(JobId, Status) to controller
    |            | GenerateAIResponse(JobId, Status="Pending")
    |            ↓
    |        (--API-----------------------------)
    |        AIController.Generate(AIRequest) wraps GenerateAIResponse in ApiResponse
    |            |
    |            | ApiResponse<GenerateAIResponse>(JobId, Status, "AI generation job created")
    |            ↓
    |        Client - receives GenerateAIResponse(JobId, Status="Pending")
    ↓
(--Infrastructure-------------------)
AIWorker.ExecuteAsync() - background service that continuously checks the in-memory queue for new AIJobs to process
    |
    | AIJob dequeued from in-memory queue
    ↓
AIQueue.Dequeue() - removes AIJob from in-memory queue for processing
    |
    | AIJob(Id, ProjectId, JobType, Prompt, Status="Pending", RetryCount=0, MaxRetries=3, CreatedAt)
    ↓
AIWorker.ExecuteAsync(AIJob) - create AIRequest from AIJob, calls IAIService.GenerateAsync(AIRequest) to process the job, updates AIJob status to "Processing", then updates status to "Completed" or "Failed" based on result
    |
    |
    ↓
IAIService.GenerateAsync(AIRequest) - 
    |
    |
    ↓
AiService.GenerateAsync(AIRequest) -     |
    | cacheKey = $"ai:{request.Prompt}"
    ↓
ICacheService.GetAsync(cacheKey) 
    |
    |
    ↓
(---Redis-----------------------------)
      -------Redis GET cacheKey---------
     /                                  \
    /                                    \
   /                                      \
  /                                        \
Cache Hit                               Cache Miss
cachedResult                             AiService.GenerateAsync(AIRequest)
   |                                        |
   |                                        ↓                    
AiService.GenerateAsync(AIRequest)       IAIProvider.GenerateAsync(Prompt)
    |                                       |
    | AIResult (string output)              ↓
    ↓                                    (--Infrastructure-------------------)  
AIResponse(AIResult, IsFallback=false)   GeminiAIProvider.GenerateAsync(Prompt) - calls Gemini API to generate AI response based on prompt, returns AI result as string
                                            |
                                            | Gemini API key from configuration


-#-#-#-#-#-#-#-#-#-#-#-#-#-#-#-#-#-#-#-#-#-#-#-#-#-#-#-#-#-#-#-#-#-#-#-#-#-#-#-#-#-#-#-#-#-#-#-#-#-#-#-#-#-#-#-#-#-#-#-#-#-#-#-#-#-#-#-#-#-#-#-#-#-#-#-#-#-#-#-#-#-#-#-#-#-#-#-#-#-#-#-#-#-#-#-#-#-#-#-#-#-#-#-#-#-#-#-#-#-#-#-#-#-#-#-#-#-#-#-#


2. Get Status endpoint:
   Client
    |
    | requests status of AI generation job with JobId
    ↓ 
(---API-----------------------------)
user requests status of AI generation job with JobId
    |
    | JobStatusRequest(JobId)
    ↓ 
(----API---------------------------------)
AIController.GetStatus(Guid jobId) - converts JobStatusRequest to GetAIStatusQuery via MediatR
    |
    | GetAIStatusQuery(JobId)
    ↓
MediatR - routes GetAIStatusQuery to GetAIStatusQueryHandler
    |
    | GetAIStatusQuery(JobId)
    ↓
(---Application---------------------)
GetAIStatusQueryHandler.Handle(GetAIStatusQuery) - retrieves AIJob from IAIJobStore using JobId, returns AIJobStatusResponse with current status
    |
    | AIJobStatusResponse(JobId, Status)
    ↓
IAIJobStore (Application)
    |
    | call AIJobStore.GetJobByIdAsync(JobId)
    ↓
(---Infrastructure-------------------)
AIJobStore (Infrastructure)
    |
    | queries database using AppDbContext to fetch AIJob by JobId and maps result to AIJob entity
    ↓
(---Database--------------------------)
DbContext - executes query to get AIJob from database and maps result to AIJob entity
    | AIJob Entity in Core
    ↓
--Application--------------------------
GetAIStatusQueryHandler.Handle(GetAIStatusQuery) - receives AIJob entity, extracts status, returns AIJobStatusResponse to client
    | AIJobStatusResponse(JobId, Status)
    ↓
AIController wraps AIJobStatusResponse in ApiResponse
    |
    | ApiResponse<AIJobStatusResponse>(JobId, Status, "AI job status retrieved")
    ↓
Client - receives current status of AI generation job (e.g. "Pending", "In Progress", "Completed", "Failed")
*/

using Application.Common.Exceptions;
using Application.DTOs.AI;
using Application.Interfaces;
using Core.Entities;
using Application.Common.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using MediatR;
using Application.Features.AI.Commands.GenerateAI;
using System.Threading.Tasks;
using Application.Features.AI.Queries.GetAIStatus;
using Application.Features.AI.Queries.GetAIJobs;

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
        var result = await _mediator.Send(
            new GenerateAICommand
            {
                Prompt = request.Prompt
            });

        // returns GenerateAIResponse(JobId, Status) to client
        return Ok(ApiResponse<GenerateAIResponse>
            .SuccessResponse(result, "AI generation job created successfully."));
    }

    [Authorize]
    [HttpGet("status/{jobId}")]
    public async Task<IActionResult> GetStatus(Guid jobId)
    {
        var result = await _mediator.Send(
            new GetAIStatusQuery
            {
                JobId = jobId
            });

        // returns AIJobStatusResponse(JobId, Status) to client
        return Ok(ApiResponse<AIStatusResponse>
            .SuccessResponse(result, "AI job status retrieved successfully."));
    }

    [Authorize]
    [HttpGet("jobs")]
    public async Task<IActionResult> GetJobs(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10)
    {
        var result = await _mediator.Send(
            new GetAIJobsQuery
            {
                Page = page,
                PageSize = pageSize
            });

        return Ok(ApiResponse<List<GetAIJobsResponse>>
            .SuccessResponse(result, "AI jobs retrieved successfully."));
    }
}