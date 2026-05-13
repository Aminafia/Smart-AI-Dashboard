/*
    Request comes in
    ↓
    CorrelationMiddleware
    → Generate ID
    → Store in context
    → Add to response
    ↓
    All other middleware + controller
    ↓
    Every log uses same ID
    ↓
    Response returns with same ID
*/

using Microsoft.Extensions.Logging; // Provides ILogger for structured logging
using Serilog.Context;

namespace API.Middlewares;

/// <summary>
/// Middleware that assigns a unique Correlation ID to each request.
/// This ID is used to trace a request across logs and system components.
/// </summary>
public class CorrelationIdMiddleware
{
    private readonly RequestDelegate _next; // Reference to the NEXT middleware in the pipeline, so control moves forward.
    private readonly ILogger<CorrelationIdMiddleware> _logger; // Logger instance used to log correlation IDs, Injected via Dependency Injection.
    private const string HeaderName = "X-Correlation-ID"; //Standard header name used across request/response. This is the key identifier for correlation ID in logs and headers.
    // Constructor to inject dependencies (RequestDelegate and ILogger)
    public CorrelationIdMiddleware(RequestDelegate next, ILogger<CorrelationIdMiddleware> logger)
    {
        _next = next; // Points to next middleware in pipeline
        _logger = logger;  // Logging service
    }

    /// <summary>
    /// Main execution method of middleware, that runs for EVERY incoming HTTP request.
    /// 1. Check if incoming request has a Correlation ID header. If yes, use it. If no, generate a new unique GUID.
    /// 2. Save the correlation Id in request memory (HttpContext.Items is the per-request storage dictionary) so it can be accessed by other middlewares and controllers during the request processing.
    /// 3. Add the correlation ID to the HTTP response headers, so clients and downstream services can also use it for tracing.
    /// 4. Log the assigned correlation ID for the incoming request for debugging and tracing in logs.
    /// 5. Pass control to the next middleware in the pipeline to continue processing the request.
    public async Task Invoke(HttpContext context)
    {
        var correlationId = context.Request.Headers[HeaderName].FirstOrDefault()
                            ?? Guid.NewGuid().ToString();

        context.Items[HeaderName] = correlationId;

        context.Response.Headers[HeaderName] = correlationId;

        using (LogContext.PushProperty("CorrelationId", correlationId))
        {
            _logger.LogInformation(
                "[Correlation] Assigned CorrelationId: {CorrelationId}",
                correlationId);

            await _next(context);
        }
    }
}