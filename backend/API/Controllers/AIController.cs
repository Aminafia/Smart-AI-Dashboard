/*
-#-#-#-#-#-#-#-#-#-#-#-#-#-#-#-#-#-#-#-#-#-#-#-#-#-#-#-#-#-#-#-#-#-#-#-#-#-#-#-#-#-#-#-#-#-#-#-#-#-#-#-#-#-#
1. Generete endpoint:
   User
    |
    |
GenerateTextRequest(Prompt)
    |
    | 
    ↓ 
(---API-----------------------------)
AIController.Generate(AIRequest) - converts AIRequest to GenerateAICommand via MediatR
    |
    | GenerateTextCommand(Prompt)
    ↓
MediatR - routes GenerateTextCommand to GenerateTextCommandHandler
    |
    | GenerateTextCommand(Prompt)
    ↓
(---Application---------------------)
GenerateTextCommandHandler.Handle(GenerateTextCommand) - creates new AIJob with status "Pending",
    |            |                                              - adds this AIJob to IAIJobStore,
    |            |                                              - enqueues job in IAIQueue for processing     
                 |                                              - return AIOperationResponse                                      - enqueues job in IAIQueue for processing
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
    |            |                                              -  returns AIOperationResponse(JObId, Status="Pending", Output="", isFallback=false) to controller
    |            | AIOperationResponse(JObId, Status="Pending", Output="", isFallback=false) 
    |            ↓
    |        (--API-----------------------------)
    |        AIController.Generate(AIRequest) wraps GenerateAIResponse in ApiResponse
    |            |
    |            | ApiResponse<AIOperationResponse>
    |            ↓
    |        Client - receives AIOperationResponse(JobId, Status="Pending")
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
AIWorker.ExecuteAsync(AIJob) - create AIRequest from AIJob, calls IAIService.ProcessAsync(AIRequest) to process the job, updates AIJob status to "Processing", then updates status to "Completed" or "Failed" based on result
    |
    |
    ↓
AiService.GenerateAsync(AIRequest) -     |
    | cacheKey = $"ai:{request.JobType}:{request.Input}"
    ↓
ICacheService.GetAsync(cacheKey) 
    |
    |
    ↓
(---Redis-----------------------------)
      -------Redis GET cacheKey---------
     /                                         \
    /                                           \
   /                                             \
  /                                               \
Cache Hit                                      Cache Miss
cachedResult                                    AiService.GenerateAsync(AIRequest)
   |                                               |
   |                                               ↓                    
AiService.GenerateAsync(AIRequest)              IAIProvider.GenerateAsync(Prompt)
    |                                              |
    | AIResult (string output)                     ↓
    ↓                                            (--Infrastructure-------------------)  
AIProviderResponse(Output,IsFallback=false)      GeminiProvider.GenerateAsync(Prompt) - calls Gemini API to generate AI response based on prompt, returns AI result as string
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
    | AIStatusResponse(JobId, Status)
    ↓
AIController wraps AIStatusResponse in ApiResponse
    |
    | ApiResponse<AIStatusResponse>(JobId, Status, "AI job status retrieved")
    ↓
Client - receives current status of AI generation job (e.g. "Pending", "In Progress", "Completed", "Failed")


+++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
Summarize Endpoint
+++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++

User
 |
SummarizeRequest(Text)
 |
 ↓
API
 |
AIController.Summarize()
 |
 ↓
SummarizeCommand(Text)
 |
 ↓
MediatR
 |
 ↓
SummarizeCommandHandler
 |
 ↓
AIRequest(Input, JobType = Summarize)
 |
 ↓
IAIService.ProcessAsync()
 |
 ↓
AiService.ProcessAsync()
 |
 ↓
Redis Cache
 |
 |---- Cache Hit ----------------------|
 |                                     |
 |                                     ↓
 |                           AIProviderResponse
 |
 |---- Cache Miss ---------------------|
                                       |
                                       ↓
                             Build Prompt
                             ("Summarize...")
                                       |
                                       ↓
                             GeminiProvider
                                       |
                                       ↓
                             AIProviderResponse
                                       |
                                       ↓
SummarizeCommandHandler converts
AIProviderResponse
        ↓
AIOperationResponse
(
Status=Completed,
Output,
IsFallback
)
        ↓
Controller
        ↓
ApiResponse<AIOperationResponse>
        ↓
Client
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
using Application.Features.AI.Commands.GenerateText;
using System.Threading.Tasks;
using Application.Features.AI.Queries.GetAIStatus;
using Application.Features.AI.Queries.GetAIJobs;
using Application.Features.AI.Commands.Summarize;
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
    public async Task<IActionResult> Generate(GenerateTextRequest request)
    {
        var result = await _mediator.Send(
            new GenerateTextCommand
            {
                Prompt = request.Prompt
            });

        // returns GenerateTextResponse(JobId, Status) to client
        return Ok(ApiResponse<AIOperationResponse>
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
}