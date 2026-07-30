using Application.Common.Models;
using Application.Features.Documents.Models;
using Application.Interfaces;
using MediatR;

namespace Application.Features.Documents.Queries.GetDocuments;

public class GetDocumentsQueryHandler
    : IRequestHandler<GetDocumentsQuery, PagedResponse<DocumentResponse>>
{
    private readonly IDocumentRepository _documentRepository;

    public GetDocumentsQueryHandler(IDocumentRepository documentRepository)
    {
        _documentRepository = documentRepository;
    }

    public async Task<PagedResponse<DocumentResponse>> Handle(
        GetDocumentsQuery request,
        CancellationToken cancellationToken)
    {
        var documents = await _documentRepository.GetPagedAsync(
            request.Page,
            request.PageSize);

        return new PagedResponse<DocumentResponse>
        {
            Items = documents.Items.Select(d => new DocumentResponse
            {
                Id = d.Id,
                FileName = d.FileName,
                ContentType = d.ContentType,
                FileSize = d.FileSize,
                UploadedAt = d.UploadedAt
            }).ToList(),

            Page = documents.Page,
            PageSize = documents.PageSize,
            TotalCount = documents.TotalCount
        };
    }
}