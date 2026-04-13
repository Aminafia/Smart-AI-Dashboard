using Serilog;
using System.Diagnostics;

namespace API.Middlewares;
public class RequestLoggingMiddleware
{
    private readonly RequestDelegate _next;

    public RequestLoggingMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task Invoke(HttpContext context)
    {
        var correlationId = context.Items["X-Correlation-ID"]?.ToString();

        var stopwatch = Stopwatch.StartNew();

        try
        {
            await _next(context);

            stopwatch.Stop();

            Log.Information(
                "HTTP {Method} {Path} responded {StatusCode} in {Duration} ms | CorrelationId: {CorrelationId}",
                context.Request.Method,
                context.Request.Path,
                context.Response.StatusCode,
                stopwatch.ElapsedMilliseconds,
                correlationId
            );
        }
        catch (Exception ex)
        {
            stopwatch.Stop();

            Log.Error(ex,
                "HTTP {Method} {Path} FAILED in {Duration} ms | CorrelationId: {CorrelationId}",
                context.Request.Method,
                context.Request.Path,
                stopwatch.ElapsedMilliseconds,
                correlationId
            );

            throw;
        }
    }
}