namespace PRN232.LMS.Services.Common;

public class ServiceResult<T>
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public T? Data { get; set; }
    public List<string>? Errors { get; set; }
    public int StatusCode { get; set; } = 200;

    public static ServiceResult<T> Ok(T data, string message = "Request processed successfully")
    {
        return new ServiceResult<T>
        {
            Success = true,
            StatusCode = 200,
            Message = message,
            Data = data
        };
    }

    public static ServiceResult<T> NotFound(string message, List<string>? errors = null)
    {
        return new ServiceResult<T>
        {
            Success = false,
            StatusCode = 404,
            Message = message,
            Errors = errors
        };
    }

    public static ServiceResult<T> BadRequest(string message, List<string>? errors = null)
    {
        return new ServiceResult<T>
        {
            Success = false,
            StatusCode = 400,
            Message = message,
            Errors = errors
        };
    }
}

public class PagedResult<T>
{
    public T Data { get; set; } = default!;
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalItems { get; set; }
    public int TotalPages { get; set; }
}
