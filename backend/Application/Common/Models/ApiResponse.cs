/*
There are 2 main scenarios where `ApiResponse<T>` is used in the application:
1. Successful API Response:
   - When an API endpoint successfully processes a request, it wraps the result in an `ApiResponse<T>` object with `Success` set to `true`, the actual data in the `Data` property, and an optional success message.
   - It is used in controllers to standardize the structure of successful responses sent back to clients, ensuring consistency across all endpoints.
   - Example: After a successful login, the `AuthController` returns an `ApiResponse<LoginResponse>` containing the JWT token and a success message.
   Success Path:
        Controller
        ↓
        Mediator.Send()
        ↓
        Handler succeeds
        ↓
        Return SuccessResponse()

2. Failed API Response:
   - When an API endpoint encounters an error while processing a request, it wraps the error information in an `ApiResponse<T>` object with `Success` set to `false`, an error message, and a list of error details.
   - It is used in the `ExceptionHandlingMiddleware` to standardize the structure of error responses sent back to clients, ensuring that all errors are returned in a consistent format regardless of where they occur in the application.
   - Example: If a user tries to log in with invalid credentials, the `AuthController` returns an `ApiResponse<LoginResponse>` with `Success` set to `false`, an error message, and a list of validation errors.
    Failure Path:
        Exception thrown
        ↓
        Controller execution stops immediately
        ↓
        RequestLoggingMiddleware logs error
        ↓
        ExceptionHandlingMiddleware catches exception
        ↓
        Creates Failure response
        ↓
        Returns standardized error response
*/

namespace Application.Common.Models;

public class ApiResponse<T>
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public T? Data { get; set; }
    public List<string>? Errors { get; set; }
    public static ApiResponse<T> SuccessResponse(T data, string message = "")
        => new ApiResponse<T>
        {
            Success = true,
            Message = message,
            Data = data
        };

    public static ApiResponse<T> Failure(string message, List<string>? errors = null)
        => new ApiResponse<T>
        {
            Success = false,
            Message = message,
            Errors = errors
        };
}