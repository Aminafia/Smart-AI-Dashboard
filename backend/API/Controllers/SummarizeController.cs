using Microsoft.AspNetCore.Mvc;
using API.Models;
using API.Services;

namespace API.Controllers
{
    [ApiController]
    [Route("api/summarize")]
    public class SummarizeController : ControllerBase
    {
        private readonly IAiService _aiService;

        public SummarizeController(IAiService aiService)
        {
            _aiService = aiService;
        }

        [HttpPost]
        public async Task<IActionResult> Summarize([FromBody] SummarizeRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Text))
                return BadRequest("Text cannot be empty");
        
            var result = await _aiService.SummarizeAsync(request);
            return Ok(result);
        }
    }
}