using Application.DTOs.Documents;
using Application.Features.Documents.Commands.UploadDocument;
using Application.Features.Documents.Queries.GetDocuments;
using Application.Features.Documents.Queries.GetDocument;
using Application.Features.Documents.Models;
using Application.Features.Documents.Commands.DeleteDocument;
using Application.Common.Models;
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
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<DocumentResponse>> Upload(
        IFormFile file)
    {
        if (file == null || file.Length == 0)
        {
            return BadRequest("No file was uploaded.");
        }

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


    [HttpGet]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PagedResponse<DocumentResponse>>> GetDocuments(
    [FromQuery] GetDocumentsQuery query)
    {
        var response = await _mediator.Send(query);

        return Ok(response);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetDocument(Guid id)
    {
        var document = await _mediator.Send(new GetDocumentQuery
        {
            Id = id
        });

        return File(
            document.Content,
            document.ContentType,
            document.FileName);
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteDocument(Guid id)
    {
        await _mediator.Send(new DeleteDocumentCommand
        {
            Id = id
        });

        return NoContent();
    }
}