/*
    Request comes in
        ↓
    ExceptionMiddleware starts (try block)
        ↓
    Everything else runs (controller, DB, etc.)
        ↓
    IF error happens → comes back here
        ↓
    Catch block handles it
        ↓
    Return clean JSON response
*/

using System.Text.Json; // Used to convert ApiResponse object into JSON string
using Application.Common.Exceptions; // Custom exceptions
using Application.Common.Models; // ApiResponse<T> wrapper for consistent API output
using FluentValidation; // ValidationException comes from here

namespace API.Middlewares;

/// <summary>
/// Global exception handling middleware.
/// Wraps the entire request pipeline and catches ALL exceptions thrown anywhere downstream.
/// Converts exceptions into clean, consistent JSON API responses.
/// </summary>
public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next; // Reference to the NEXT middleware in the pipeline, so control moves forward.
    private readonly ILogger<ExceptionHandlingMiddleware> _logger; // Logger instance used to log warnings/errors, Injected via Dependency Injection.

    // Constructor — ASP.NET injects dependencies automatically.
    public ExceptionHandlingMiddleware(
        RequestDelegate next,
        ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;       // Points to next middleware in pipeline
        _logger = logger;   // Logging service
    }

    /// <summary>
    /// Main execution method of middleware, that runs for EVERY incoming HTTP request.
    /// </summary>
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            /// <summary>
            /// Pass control to the NEXT middleware in pipeline.
            /// This is where the entire request (controller, DB, etc.) executes.
            /// If ANY exception happens later, it will come back here.
            /// </summary>
            await _next(context);
        }

        /// <summary>
        /// Handles FluentValidation errors (input validation failures, wich are user input mistakes not system errors. Example: Invalid email format, password too short).
        /// 1. Retrieve correlation ids for tracing requets in logs.
        /// 2. Log validation error with warning level.
        /// 3. Set HTTP response status and content type.
        /// 4. Extract individual validation error messages.
        /// 5. Wrap errors in standardized API response format.
        /// 6. Serialize response to JSON and send back to client.
        /// </summary>
        catch (ValidationException ex)
        {
            var correlationId = context.Items["X-Correlation-ID"];

            _logger.LogWarning(ex,
                "Validation error | Path: {Path} | CorrelationId: {CorrelationId}",
                context.Request.Path, correlationId);

            context.Response.StatusCode = StatusCodes.Status400BadRequest;
            context.Response.ContentType = "application/json";

            var errors = ex.Errors
                .Select(e => e.ErrorMessage)
                .ToList();

            var apiResponse = ApiResponse<string>.Failure("Validation failed", errors);

            await context.Response.WriteAsync(JsonSerializer.Serialize(apiResponse));
        }

        /// <summary>
        /// Handles ALL other exceptions in the system (system, DB, business logic, etc.)
        /// 1. Log full exception with error level, including request path and correlation id for debugging.
        /// 2. Delegate handling to centralized method HandleExceptionAsync.
        /// </summary>
        catch (Exception ex)
        {
            var correlationId = context.Items["X-Correlation-ID"];

            _logger.LogError(ex,
                "Unhandled exception | Path: {Path} | CorrelationId: {CorrelationId}",
                context.Request.Path,
                correlationId);

            await HandleExceptionAsync(context, ex);
        }
    }

    /// <summary>
    /// Maps different exception types to appropriate HTTP responses.
    /// Ensures consistent API structure for all errors.
    /// </summary>
    private async Task HandleExceptionAsync(HttpContext context, Exception ex)
    {
        /// <summary>
        /// 1. Shortcut reference to HTTP response.
        /// 2. Return response as JSON for ALL errors.
        /// 3. Retrieve correlation ID again for logging.
        /// 4. Declare variable to hold standardized API response object.
        /// </summary>
        var response = context.Response;

        response.ContentType = "application/json";

        var correlationId = context.Items["X-Correlation-ID"];

        ApiResponse<string> apiResponse;

        /// <summary>
        /// Map specific exception types to HTTP status codes and response messages.
        /// </summary>
        (response.StatusCode, apiResponse) = ex switch
        {
            BadRequestException =>
                (
                    StatusCodes.Status400BadRequest,
                    ApiResponse<string>.Failure(ex.Message)
                ),

            UnauthorizedException =>
                (
                    StatusCodes.Status401Unauthorized,
                    ApiResponse<string>.Failure(ex.Message)
                ),

            NotFoundException =>
                (
                    StatusCodes.Status404NotFound,
                    ApiResponse<string>.Failure(ex.Message)
                ),

            DuplicateEmailException =>
                (
                    StatusCodes.Status409Conflict,
                    ApiResponse<string>.Failure(ex.Message)
                ),

            _ =>
                (
                    StatusCodes.Status500InternalServerError,
                    ApiResponse<string>.Failure("Internal Server Error")
                )
        };

        /// <summary>
        /// Final logging AFTER exception is processed.
        /// This confirms what response was returned.
        /// 1. Log the final status code and correlation id for tracing in logs.
        /// 2. Convert response object to JSON string.
        /// 3. Send JSON response back to client.
        /// </summary>
        _logger.LogError(ex,
            "[ExceptionHandler] Processed exception | StatusCode: {StatusCode} | CorrelationId: {CorrelationId}",
            response.StatusCode,
            correlationId);

        var json = JsonSerializer.Serialize(apiResponse);

        await response.WriteAsync(json);
    }
}