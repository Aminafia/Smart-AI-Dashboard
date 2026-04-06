namespace Application.Common.Models;

public class ApiResponse<T>
{
    public bool Success { get; set; }
    public string Message { get; set; }
    public List<string>? Errors { get; set; }
    public T? Data { get; set; }

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