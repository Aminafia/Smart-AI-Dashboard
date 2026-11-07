using System.Text.Json;

using API.Middlewares;
using Application.Common.Exceptions;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;

namespace Application.Tests.API;

public class ExceptionHandlingMiddlewareTests
{
    private readonly Mock<ILogger<ExceptionHandlingMiddleware>> _logger = new();

    [Theory]
    [InlineData(400, "Bad request")]
    [InlineData(401, "Not authorized")]
    [InlineData(404, "Document not found")]
    [InlineData(409, "Email already exists")]
    public async Task InvokeAsync_MapsKnownExceptionsToExpectedStatusCodes(int statusCode, string message)
    {
        Exception exception = statusCode switch
        {
            400 => new BadRequestException(message),
            401 => new UnauthorizedException(message),
            404 => new NotFoundException(message),
            409 => new DuplicateEmailException(message),
            _ => throw new ArgumentOutOfRangeException(nameof(statusCode))
        };
        var context = CreateContext();
        var middleware = new ExceptionHandlingMiddleware(_ => throw exception, _logger.Object);

        await middleware.InvokeAsync(context);

        Assert.Equal(statusCode, context.Response.StatusCode);
        Assert.Equal("application/json", context.Response.ContentType);
        var body = await ReadBodyAsync(context);
        Assert.Contains(message, body);
    }

    [Fact]
    public async Task InvokeAsync_ValidationException_ReturnsBadRequestWithAllMessages()
    {
        var exception = new ValidationException(
        [
            new ValidationFailure("Email", "Email is invalid."),
            new ValidationFailure("Password", "Password is required.")
        ]);
        var context = CreateContext();
        var middleware = new ExceptionHandlingMiddleware(_ => throw exception, _logger.Object);

        await middleware.InvokeAsync(context);

        Assert.Equal(StatusCodes.Status400BadRequest, context.Response.StatusCode);
        var body = await ReadBodyAsync(context);
        Assert.Contains("Validation failed", body);
        Assert.Contains("Email is invalid.", body);
        Assert.Contains("Password is required.", body);
    }

    [Fact]
    public async Task InvokeAsync_UnexpectedException_ReturnsGeneric500WithoutLeakingDetails()
    {
        var context = CreateContext();
        var middleware = new ExceptionHandlingMiddleware(
            _ => throw new InvalidOperationException("Sensitive database detail"),
            _logger.Object);

        await middleware.InvokeAsync(context);

        Assert.Equal(StatusCodes.Status500InternalServerError, context.Response.StatusCode);
        var body = await ReadBodyAsync(context);
        Assert.Contains("Internal Server Error", body);
        Assert.DoesNotContain("Sensitive database detail", body);
    }

    [Fact]
    public async Task InvokeAsync_SuccessfulRequest_PassesThroughWithoutChangingResponse()
    {
        var context = CreateContext();
        var middleware = new ExceptionHandlingMiddleware(
            httpContext =>
            {
                httpContext.Response.StatusCode = StatusCodes.Status202Accepted;
                return Task.CompletedTask;
            },
            _logger.Object);

        await middleware.InvokeAsync(context);

        Assert.Equal(StatusCodes.Status202Accepted, context.Response.StatusCode);
        Assert.Equal(string.Empty, await ReadBodyAsync(context));
    }

    private static DefaultHttpContext CreateContext()
    {
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();
        context.Items["X-Correlation-ID"] = "test-correlation-id";
        return context;
    }

    private static async Task<string> ReadBodyAsync(HttpContext context)
    {
        context.Response.Body.Position = 0;
        using var reader = new StreamReader(context.Response.Body);
        return await reader.ReadToEndAsync();
    }
}
