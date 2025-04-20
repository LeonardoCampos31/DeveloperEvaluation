namespace Sales.Api.Models;

public class ApiResponse<T>
{
    public T? Data { get; set; }
    public string Status { get; set; } // "success", "error"
    public string Message { get; set; }
    public object? Errors { get; set; } // Optional: For validation errors

    public ApiResponse(T? data, string message = "Operação concluída com sucesso", string status = "success", object? errors = null)
    {
        Data = data;
        Status = status;
        Message = message;
        Errors = errors;
    }

    public static ApiResponse<T> Success(T data, string message = "Operação concluída com sucesso")
    {
        return new ApiResponse<T>(data, message, "success");
    }

    public static ApiResponse<object> Error(string message, string type = "error", object? errors = null)
    {
        return new ApiResponse<object>(null, message, type, errors);
    }
    public static ApiResponse<object> SuccessMessage(string message = "Operação concluída com sucesso")
    {
        return new ApiResponse<object>(null, message, "success");
    }
}