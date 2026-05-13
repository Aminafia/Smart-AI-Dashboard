/*
    Request →
    CorrelationMiddleware → adds ID
    → LoggingMiddleware START log
    → Controller + Handler + DB
    → LoggingMiddleware END log
    → Response
*/

using System.Diagnostics;
using Serilog;

namespace API.Middlewares;

/// <summary>
/// Middleware that logs incoming requests and outgoing responses.
/// Measures execution time and tracks full request lifecycle.
/// </summary>
public class RequestLoggingMiddleware
{
    private readonly RequestDelegate _next; // Next middleware in the pipeline
    // Constructor to inject the next middleware
    public RequestLoggingMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    /// <summary>
    /// Invoked for each HTTP request. Logs request details, measures execution time, and logs response status.
    /// </summary>
    /// <param name="context"></param>
    /// <returns></returns>
    /// 1. Get correlation ID from previous CorrelationIdMiddleware.
    /// 2. Start a stopwatch to measure request processing time.
    /// 3. Log incoming request details (method, path, correlation ID).
    /// 4. Call the next middleware in the pipeline to continue processing the request.
    /// 5. After the next middleware (and ultimately the controller) has processed the request, stop the stopwatch and log the response status code and execution time.
    /// 6. If any exception occurs during processing, catch it, log the error with correlation ID and execution time, and rethrow the exception to be handled by global error handling middleware.
    public async Task Invoke(HttpContext context)
    {
        var stopwatch = Stopwatch.StartNew();

        Log.Information(
            "[Middleware] Incoming Request: {Method} {Path}",
            context.Request.Method,
            context.Request.Path
        );

        try
        {
            await _next(context);
            stopwatch.Stop();

            Log.Information(
                "[Middleware] Response: {StatusCode} in {Duration} ms",
                context.Response.StatusCode,
                stopwatch.ElapsedMilliseconds
            );
        }

        catch (Exception ex)
        {
            stopwatch.Stop();

            Log.Error(ex,
                "[Middleware] FAILED: {Method} {Path} in {Duration} ms",
                context.Request.Method,
                context.Request.Path,
                stopwatch.ElapsedMilliseconds
            );

            throw; // Thrown to be caught by ExceptionHandlingMiddleware, which will return standardized error response to client. 
        }
    }
}