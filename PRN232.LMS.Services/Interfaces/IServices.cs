using PRN232.LMS.Services.BusinessModels;
using PRN232.LMS.Services.Common;
using PRN232.LMS.Services.Requests;
using PRN232.LMS.Services.Responses;

namespace PRN232.LMS.Services.Interfaces;

public interface ISemesterService
{
    Task<PaginatedResponse<List<SemesterResponse>>> GetAllAsync(QueryParams queryParams);
    Task<ApiResponse<SemesterResponse>> GetByIdAsync(int id);
    Task<ApiResponse<SemesterResponse>> CreateAsync(CreateSemesterRequest request);
    Task<ApiResponse<SemesterResponse>> UpdateAsync(int id, UpdateSemesterRequest request);
    Task<ApiResponse<bool>> DeleteAsync(int id);
}

public interface ICourseService
{
    Task<PaginatedResponse<List<CourseResponse>>> GetAllAsync(QueryParams queryParams);
    Task<ApiResponse<CourseResponse>> GetByIdAsync(int id, string? expand = null);
    Task<ApiResponse<CourseResponse>> CreateAsync(CreateCourseRequest request);
    Task<ApiResponse<CourseResponse>> UpdateAsync(int id, UpdateCourseRequest request);
    Task<ApiResponse<bool>> DeleteAsync(int id);
}

public interface IStudentService
{
    Task<PaginatedResponse<List<StudentResponse>>> GetAllAsync(QueryParams queryParams);
    Task<ApiResponse<StudentResponse>> GetByIdAsync(int id, string? expand = null);
    Task<ApiResponse<StudentResponse>> CreateAsync(CreateStudentRequest request);
    Task<ApiResponse<StudentResponse>> UpdateAsync(int id, UpdateStudentRequest request);
    Task<ApiResponse<bool>> DeleteAsync(int id);
}

public interface IEnrollmentService
{
    Task<PaginatedResponse<List<EnrollmentResponse>>> GetAllAsync(QueryParams queryParams);
    Task<ApiResponse<EnrollmentResponse>> GetByIdAsync(int id, string? expand = null);
    Task<ApiResponse<EnrollmentResponse>> CreateAsync(CreateEnrollmentRequest request);
    Task<ApiResponse<EnrollmentResponse>> UpdateAsync(int id, UpdateEnrollmentRequest request);
    Task<ApiResponse<bool>> DeleteAsync(int id);
}

public interface ISubjectService
{
    Task<PaginatedResponse<List<SubjectResponse>>> GetAllAsync(QueryParams queryParams);
    Task<ApiResponse<SubjectResponse>> GetByIdAsync(int id);
    Task<ApiResponse<SubjectResponse>> CreateAsync(CreateSubjectRequest request);
    Task<ApiResponse<SubjectResponse>> UpdateAsync(int id, UpdateSubjectRequest request);
    Task<ApiResponse<bool>> DeleteAsync(int id);
}
