using Application.Common.Models;
using Application.Features.Documents.Models;
using MediatR;

namespace Application.Features.Documents.Queries.GetDocuments;

public class GetDocumentsQuery
    : IRequest<PagedResponse<DocumentResponse>>
{
    public int Page { get; set; } = 1;

    public int PageSize { get; set; } = 10;
}