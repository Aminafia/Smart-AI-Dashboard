using API.Middlewares;

using Microsoft.AspNetCore.Http;

namespace Application.Tests.API;

public class RequestLoggingMiddlewareTests
{
    [Fact]
    public async Task Invoke_SuccessfulRequest_ExecutesNextMiddleware()
    {
        var context = new DefaultHttpContext();
        var nextCalled = false;
        var middleware = new RequestLoggingMiddleware(_ =>
        {
            nextCalled = true;
            return Task.CompletedTask;
        });

        await middleware.Invoke(context);

        Assert.True(nextCalled);
    }

    [Fact]
    public async Task Invoke_DownstreamException_RethrowsOriginalException()
    {
        var context = new DefaultHttpContext();
        var expected = new InvalidOperationException("Downstream failure");
        var middleware = new RequestLoggingMiddleware(_ => throw expected);

        var actual = await Assert.ThrowsAsync<InvalidOperationException>(
            () => middleware.Invoke(context));

        Assert.Same(expected, actual);
    }
}
