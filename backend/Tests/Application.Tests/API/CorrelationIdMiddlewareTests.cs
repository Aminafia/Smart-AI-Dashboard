using API.Middlewares;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;

namespace Application.Tests.API;

public class CorrelationIdMiddlewareTests
{
    private readonly Mock<ILogger<CorrelationIdMiddleware>> _logger = new();

    [Fact]
    public async Task Invoke_ExistingHeader_PreservesAndReturnsClientCorrelationId()
    {
        var context = new DefaultHttpContext();
        context.Request.Headers["X-Correlation-ID"] = "client-correlation-id";
        var middleware = new CorrelationIdMiddleware(_ => Task.CompletedTask, _logger.Object);

        await middleware.Invoke(context);

        Assert.Equal("client-correlation-id", context.Items["X-Correlation-ID"]);
        Assert.Equal("client-correlation-id", context.Response.Headers["X-Correlation-ID"]);
    }

    [Fact]
    public async Task Invoke_MissingHeader_GeneratesGuidAndAddsItToRequestAndResponse()
    {
        var context = new DefaultHttpContext();
        var middleware = new CorrelationIdMiddleware(_ => Task.CompletedTask, _logger.Object);

        await middleware.Invoke(context);

        var correlationId = Assert.IsType<string>(context.Items["X-Correlation-ID"]);
        Assert.True(Guid.TryParse(correlationId, out _));
        Assert.Equal(correlationId, context.Response.Headers["X-Correlation-ID"]);
    }

    [Fact]
    public async Task Invoke_SetsCorrelationIdBeforeCallingNextMiddleware()
    {
        var context = new DefaultHttpContext();
        context.Request.Headers["X-Correlation-ID"] = "request-id";
        var nextSawCorrelationId = false;
        var middleware = new CorrelationIdMiddleware(
            httpContext =>
            {
                nextSawCorrelationId = Equals(httpContext.Items["X-Correlation-ID"], "request-id");
                return Task.CompletedTask;
            },
            _logger.Object);

        await middleware.Invoke(context);

        Assert.True(nextSawCorrelationId);
    }
}
