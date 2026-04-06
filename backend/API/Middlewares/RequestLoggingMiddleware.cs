using System.Diagnostics;

namespace API.Middlewares;

public class RequestLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RequestLoggingMiddleware> _logger;

    public RequestLoggingMiddleware(
        RequestDelegate next,
        ILogger<RequestLoggingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var stopwatch = Stopwatch.StartNew();

        var requestMethod = context.Request.Method;
        var requestPath = context.Request.Path;

        _logger.LogInformation("Incoming Request: {Method} {Path}", requestMethod, requestPath);

        await _next(context);

        stopwatch.Stop();

        var statusCode = context.Response.StatusCode;

        _logger.LogInformation(
            "Completed Request: {Method} {Path} → {StatusCode} in {Elapsed}ms",
            requestMethod,
            requestPath,
            statusCode,
            stopwatch.ElapsedMilliseconds);
    }
}