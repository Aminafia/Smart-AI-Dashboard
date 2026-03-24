using System.Net;
using System.Text.Json;
using FluentValidation;

namespace API.Middleware;

public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(
        RequestDelegate next,
        ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
{
    try
    {
        await _next(context);
    }
    catch (ValidationException ex)
    {
        context.Response.StatusCode = StatusCodes.Status400BadRequest;
        context.Response.ContentType = "application/json";

        var errors = ex.Errors
            .GroupBy(e => new { e.PropertyName, e.ErrorMessage })
            .Select(g => new
            {
                field = g.Key.PropertyName,
                message = g.Key.ErrorMessage
            });

        await context.Response.WriteAsJsonAsync(errors);
    }
    catch (Exception ex)
    {
        context.Response.StatusCode = StatusCodes.Status500InternalServerError;
        context.Response.ContentType = "application/json";

        await context.Response.WriteAsJsonAsync(new
        {
            error = "An unexpected error occurred.",
            message = ex.Message,
            traceId = context.TraceIdentifier
        });
    }
}

    private static Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var response = new
        {
            error = "An unexpected error occurred.",
            message = exception.Message,
            traceId = context.TraceIdentifier
        };

        var json = JsonSerializer.Serialize(response);

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;

        return context.Response.WriteAsync(json);
    }
}