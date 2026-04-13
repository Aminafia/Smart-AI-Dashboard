using System.Text.Json;
using Application.Common.Exceptions;
using Application.Common.Models;
using FluentValidation;

namespace API.Middlewares;

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
            var correlationId = context.Items["X-Correlation-ID"];

            _logger.LogWarning(ex,
                "Validation error | CorrelationId: {CorrelationId}",
                correlationId);

            context.Response.StatusCode = StatusCodes.Status400BadRequest;
            context.Response.ContentType = "application/json";

            var errors = ex.Errors
                .Select(e => e.ErrorMessage)
                .ToList();

            var apiResponse = ApiResponse<string>.Failure("Validation failed", errors);

            await context.Response.WriteAsync(JsonSerializer.Serialize(apiResponse));
        }
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

    private static async Task HandleExceptionAsync(HttpContext context, Exception ex)
    {
        var response = context.Response;
        response.ContentType = "application/json";

        ApiResponse<string> apiResponse;

        switch (ex)
        {
            case NotFoundException:
                response.StatusCode = StatusCodes.Status404NotFound;
                apiResponse = ApiResponse<string>.Failure(ex.Message);
                break;

            case UnauthorizedException:
                response.StatusCode = StatusCodes.Status401Unauthorized;
                apiResponse = ApiResponse<string>.Failure(ex.Message);
                break;

            case BadRequestException:
                response.StatusCode = StatusCodes.Status400BadRequest;
                apiResponse = ApiResponse<string>.Failure(ex.Message);
                break;

            case DuplicateEmailException:
                response.StatusCode = StatusCodes.Status400BadRequest;
                apiResponse = ApiResponse<string>.Failure(ex.Message);
                break;

            default:
                response.StatusCode = StatusCodes.Status500InternalServerError;
                apiResponse = ApiResponse<string>.Failure("Internal Server Error");
                break;
        }

        var json = JsonSerializer.Serialize(apiResponse);

        await response.WriteAsync(json);
    }
}