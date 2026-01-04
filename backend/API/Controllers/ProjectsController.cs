using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[ApiController]
[Route("api/projects")]
public class ProjectsController : ControllerBase
{
    [HttpPost]
    public IActionResult CreateProject()
    {
        return Ok("TODO");
    }

    [HttpGet("{id}")]
    public IActionResult GetProject(Guid id)
    {
        return Ok("TODO");
    }
}
