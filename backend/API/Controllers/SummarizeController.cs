using Application.DTOs.AI;
using Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [ApiController]
    [Route("api/summarize")]
    [Authorize]
    public class SummarizeController : ControllerBase
    {
        private readonly IAIService _aiService;
        private readonly ILogger<SummarizeController> _logger;
        public SummarizeController(IAIService aiService, ILogger<SummarizeController> logger)
        {
            _aiService = aiService;
            _logger = logger;
        }

        [HttpPost]
        public async Task<IActionResult> Summarize([FromBody] SummarizeRequest request)
        {
            _logger.LogInformation("[Controller] Summarize endpoint hit");

            if (string.IsNullOrWhiteSpace(request.Text))
                return BadRequest("Text cannot be empty");
            
            var aiRequest = new AIRequest
            {
                Prompt = request.Text,
            };

            var result = await _aiService.GenerateAsync(aiRequest);

            _logger.LogInformation("[Controller] Summarization completed");
 
            return Ok(result);
        }
    }
}