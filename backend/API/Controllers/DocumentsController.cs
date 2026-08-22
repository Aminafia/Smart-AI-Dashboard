using Application.Common.Models;
using Application.DTOs.Documents;
using Application.Features.Documents.Commands.DeleteDocument;
using Application.Features.Documents.Commands.UploadDocument;
using Application.Features.Documents.Models;
using Application.Features.Documents.Queries.GetDocument;
using Application.Features.Documents.Queries.GetDocuments;
using Application.Features.Documents.Commands.ExtractDocument;
using Application.Features.Documents.Queries.GetDocumentContent;
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
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<DocumentResponse>> Upload(
        IFormFile file,
        CancellationToken cancellationToken)
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

        var response = await _mediator.Send(
            command,
            cancellationToken);

        return Ok(response);
    }


    [HttpPost("{id:guid}/extract")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ExtractDocument(Guid id, CancellationToken cancellationToken)
    {
        await _mediator.Send(
            new ExtractDocumentCommand
            {
                DocumentId = id
            },
            cancellationToken);

        return NoContent();
    }

    [HttpGet("{id:guid}/content")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<DocumentContentResponse>> GetDocumentContent(
    Guid id,
    CancellationToken cancellationToken)
    {
        var response = await _mediator.Send(
            new GetDocumentContentQuery
            {
                DocumentId = id
            },
            cancellationToken);

        return Ok(response);
    }

    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResponse<DocumentResponse>>> GetDocuments(
        [FromQuery] GetDocumentsQuery query,
        CancellationToken cancellationToken)
    {
        var response = await _mediator.Send(
            query,
            cancellationToken);

        return Ok(response);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetDocument(
        Guid id,
        CancellationToken cancellationToken)
    {
        var document = await _mediator.Send(
            new GetDocumentQuery
            {
                Id = id
            },
            cancellationToken);

        return File(
            document.Content,
            document.ContentType,
            document.FileName);
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteDocument(
        Guid id,
        CancellationToken cancellationToken)
    {
        await _mediator.Send(
            new DeleteDocumentCommand
            {
                Id = id
            },
            cancellationToken);

        return NoContent();
    }
}