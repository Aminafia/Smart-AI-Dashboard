using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Application.Interfaces;
using Application.DTOs.AI;

namespace API.Controllers
{
    [ApiController]
    [Route("api/summarize")]
    [Authorize]
    public class SummarizeController : ControllerBase
    {
        private readonly IAIService _aiService;

        public SummarizeController(IAIService aiService)
        {
            _aiService = aiService;
        }

        [HttpPost]
        public async Task<IActionResult> Summarize([FromBody] SummarizeRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Text))
                return BadRequest("Text cannot be empty");
            var aiRequest = new AIRequest
            {
                Prompt = request.Text,
            };

            var result = await _aiService.GenerateAsync(aiRequest);

            return Ok(result);
        }
    }
}