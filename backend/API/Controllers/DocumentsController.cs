using Application.DTOs.Documents;
using Application.Features.Documents.Commands.UploadDocument;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DocumentsController : ControllerBase
{
    private readonly IMediator _mediator;

    public DocumentsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost]
    public async Task<ActionResult<UploadDocumentResponse>> Upload(
        IFormFile file)
    {
        if (file == null || file.Length == 0) {
            return BadRequest("No file was uploaded."); }

        await using var stream = file.OpenReadStream();

        var command = new UploadDocumentCommand
        {
            Document = new DocumentUpload
            {
                FileName = file.FileName,
                ContentType = file.ContentType,
                Content = stream
            }
        };

        var response = await _mediator.Send(command);

        return Ok(response);
    }
}