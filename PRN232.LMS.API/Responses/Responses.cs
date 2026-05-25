namespace PRN232.LMS.API.Responses;

public class ApiResponse<T>
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public T? Data { get; set; }
    public List<string>? Errors { get; set; }
    public int StatusCode { get; set; } = 200;

    public static ApiResponse<T> Ok(T data, string message = "Request processed successfully")
    {
        return new ApiResponse<T>
        {
            Success = true,
            StatusCode = 200,
            Message = message,
            Data = data
        };
    }

    public static ApiResponse<T> NotFound(string message, List<string>? errors = null)
    {
        return new ApiResponse<T>
        {
            Success = false,
            StatusCode = 404,
            Message = message,
            Errors = errors
        };
    }

    public static ApiResponse<T> BadRequest(string message, List<string>? errors = null)
    {
        return new ApiResponse<T>
        {
            Success = false,
            StatusCode = 400,
            Message = message,
            Errors = errors
        };
    }

    public static ApiResponse<T> Fail(string message, List<string>? errors = null)
    {
        return new ApiResponse<T>
        {
            Success = false,
            StatusCode = 500,
            Message = message,
            Errors = errors
        };
    }
}

public class PaginatedResponse<T>
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public T? Data { get; set; }
    public List<string>? Errors { get; set; }
    public int StatusCode { get; set; } = 200;
    public PaginationMetadata Pagination { get; set; } = new();

    public static PaginatedResponse<T> Ok(T data, PaginationMetadata pagination, string message = "Request processed successfully")
    {
        return new PaginatedResponse<T>
        {
            Success = true,
            StatusCode = 200,
            Message = message,
            Data = data,
            Pagination = pagination
        };
    }
}

public class PaginationMetadata
{
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalItems { get; set; }
    public int TotalPages { get; set; }
}

public class SemesterResponse
{
    public int SemesterId { get; set; }
    public string SemesterName { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public List<CourseResponse>? Courses { get; set; }
}

public class CourseResponse
{
    public int CourseId { get; set; }
    public string CourseName { get; set; } = string.Empty;
    public int SemesterId { get; set; }
    public string? SemesterName { get; set; }
    public List<EnrollmentResponse>? Enrollments { get; set; }
}

public class StudentResponse
{
    public int StudentId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public DateTime DateOfBirth { get; set; }
    public List<EnrollmentResponse>? Enrollments { get; set; }
}

public class EnrollmentResponse
{
    public int EnrollmentId { get; set; }
    public int StudentId { get; set; }
    public string? StudentName { get; set; }
    public int CourseId { get; set; }
    public string? CourseName { get; set; }
    public DateTime EnrollDate { get; set; }
    public string Status { get; set; } = string.Empty;
}

public class SubjectResponse
{
    public int SubjectId { get; set; }
    public string SubjectCode { get; set; } = string.Empty;
    public string SubjectName { get; set; } = string.Empty;
    public int Credit { get; set; }
}
