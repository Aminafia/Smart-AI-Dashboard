using Application.DTOs.AI;
using Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MediatR;
using Application.Features.AI.Commands.Summarize;

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

            return Ok(result);
        }
    }
}