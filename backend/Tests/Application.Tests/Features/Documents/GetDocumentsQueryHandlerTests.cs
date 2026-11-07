using Application.Common.Models;
using Application.Features.Documents.Queries.GetDocuments;
using Application.Interfaces;

using Core.Entities;

using Moq;

namespace Application.Tests.Features.Documents;

public class GetDocumentsQueryHandlerTests
{
    private readonly Mock<IDocumentRepository> _documents = new();
    private readonly Mock<ICurrentUserService> _currentUser = new();

    [Fact]
    public async Task Handle_ReturnsMappedDocumentsAndPaginationForCurrentUser()
    {
        var userId = Guid.NewGuid();
        var uploadedAt = DateTime.UtcNow;
        var document = new Document
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            FileName = "report.pdf",
            ContentType = "application/pdf",
            FileSize = 2048,
            UploadedAt = uploadedAt
        };
        _currentUser.SetupGet(x => x.UserId).Returns(userId);
        _documents.Setup(x => x.GetPagedAsync(2, 5, userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PagedResponse<Document>
            {
                Items = [document],
                Page = 2,
                PageSize = 5,
                TotalCount = 11
            });

        var result = await new GetDocumentsQueryHandler(_documents.Object, _currentUser.Object)
            .Handle(new GetDocumentsQuery { Page = 2, PageSize = 5 }, CancellationToken.None);

        var item = Assert.Single(result.Items);
        Assert.Equal(document.Id, item.Id);
        Assert.Equal(document.FileName, item.FileName);
        Assert.Equal(document.ContentType, item.ContentType);
        Assert.Equal(document.FileSize, item.FileSize);
        Assert.Equal(uploadedAt, item.UploadedAt);
        Assert.Equal(2, result.Page);
        Assert.Equal(5, result.PageSize);
        Assert.Equal(11, result.TotalCount);
    }

    [Fact]
    public async Task Handle_EmptyPage_ReturnsEmptyItemsAndPreservesCount()
    {
        var userId = Guid.NewGuid();
        _currentUser.SetupGet(x => x.UserId).Returns(userId);
        _documents.Setup(x => x.GetPagedAsync(4, 10, userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PagedResponse<Document>
            {
                Items = [],
                Page = 4,
                PageSize = 10,
                TotalCount = 31
            });

        var result = await new GetDocumentsQueryHandler(_documents.Object, _currentUser.Object)
            .Handle(new GetDocumentsQuery { Page = 4, PageSize = 10 }, CancellationToken.None);

        Assert.Empty(result.Items);
        Assert.Equal(31, result.TotalCount);
        Assert.Equal(4, result.Page);
    }
}
