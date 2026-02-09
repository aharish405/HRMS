namespace HRMS.Shared.Models;

public class Result<T>
{
    public bool Success { get; set; }
    public T? Data { get; set; }
    public string? Message { get; set; }
    public List<string> Errors { get; set; } = new();

    public static Result<T> SuccessResult(T data, string? message = null)
    {
        return new Result<T>
        {
            Success = true,
            Data = data,
            Message = message
        };
    }

    public static Result<T> FailureResult(string message, List<string>? errors = null)
    {
        return new Result<T>
        {
            Success = false,
            Message = message,
            Errors = errors ?? new List<string>()
        };
    }

    public static Result<T> FailureResult(List<string> errors)
    {
        return new Result<T>
        {
            Success = false,
            Errors = errors
        };
    }
}

public class Result
{
    public bool Success { get; set; }
    public string? Message { get; set; }
    public List<string> Errors { get; set; } = new();

    public static Result SuccessResult(string? message = null)
    {
        return new Result
        {
            Success = true,
            Message = message
        };
    }

    public static Result FailureResult(string message, List<string>? errors = null)
    {
        return new Result
        {
            Success = false,
            Message = message,
            Errors = errors ?? new List<string>()
        };
    }

    public static Result FailureResult(List<string> errors)
    {
        return new Result
        {
            Success = false,
            Errors = errors
        };
    }
}
