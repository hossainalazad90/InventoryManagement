namespace Inventory.Application.Common;

public class ApiResponse<T>
{
    public bool Success { get; set; } = true;
    public string? Code { get; set; }
    public string? Message { get; set; }
    public T? Data { get; set; }
    public IDictionary<string, string[]>? Errors { get; set; }

    public static ApiResponse<T> Ok(T data, string? message = null) => new()
    {
        Success = true,
        Data = data,
        Message = message
    };

    public static ApiResponse<T> Fail(string code, string message, IDictionary<string, string[]>? errors = null) => new()
    {
        Success = false,
        Code = code,
        Message = message,
        Errors = errors
    };
}

public class ApiResponse : ApiResponse<object>
{
    public static ApiResponse Ok(string? message = null) => new()
    {
        Success = true,
        Message = message
    };
}
