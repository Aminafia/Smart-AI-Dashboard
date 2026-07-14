using Application.Common.Models;
using Application.DTOs.AI;
using Application.Features.AI.Commands.Summarize;
using Application.Interfaces;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [ApiController]
    [Route("api/summarize")]
    [Authorize]
    public class SummarizeController : ControllerBase
    {
        private readonly IMediator _mediator;
        public SummarizeController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPost]
        public async Task<IActionResult> Summarize([FromBody] SummarizeRequest request)
        {
            var result = await _mediator.Send(new SummarizeCommand
            {
                Text = request.Text
            });
            return Ok(ApiResponse<AIResponse>
                .SuccessResponse(result, "Summary generated successfully."));
        }
    }
}