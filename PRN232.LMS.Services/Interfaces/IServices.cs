using PRN232.LMS.Services.BusinessModels;
using PRN232.LMS.Services.Common;

namespace PRN232.LMS.Services.Interfaces;

public interface ISemesterService
{
    Task<ServiceResult<PagedResult<List<SemesterBM>>>> GetAllAsync(QueryParams queryParams);
    Task<ServiceResult<SemesterBM>> GetByIdAsync(int id, string? expand = null);
    Task<ServiceResult<SemesterBM>> CreateAsync(SemesterBM request);
    Task<ServiceResult<SemesterBM>> UpdateAsync(int id, SemesterBM request);
    Task<ServiceResult<bool>> DeleteAsync(int id);
}

public interface ICourseService
{
    Task<ServiceResult<PagedResult<List<CourseBM>>>> GetAllAsync(QueryParams queryParams);
    Task<ServiceResult<CourseBM>> GetByIdAsync(int id, string? expand = null);
    Task<ServiceResult<CourseBM>> CreateAsync(CourseBM request);
    Task<ServiceResult<CourseBM>> UpdateAsync(int id, CourseBM request);
    Task<ServiceResult<bool>> DeleteAsync(int id);
}

public interface IStudentService
{
    Task<ServiceResult<PagedResult<List<StudentBM>>>> GetAllAsync(QueryParams queryParams);
    Task<ServiceResult<StudentBM>> GetByIdAsync(int id, string? expand = null);
    Task<ServiceResult<StudentBM>> CreateAsync(StudentBM request);
    Task<ServiceResult<StudentBM>> UpdateAsync(int id, StudentBM request);
    Task<ServiceResult<bool>> DeleteAsync(int id);
}

public interface IEnrollmentService
{
    Task<ServiceResult<PagedResult<List<EnrollmentBM>>>> GetAllAsync(QueryParams queryParams);
    Task<ServiceResult<PagedResult<List<EnrollmentBM>>>> GetByCourseIdAsync(int courseId, int page, int size);
    Task<ServiceResult<EnrollmentBM>> GetByIdAsync(int id, string? expand = null);
    Task<ServiceResult<EnrollmentBM>> CreateAsync(EnrollmentBM request);
    Task<ServiceResult<EnrollmentBM>> UpdateAsync(int id, EnrollmentBM request);
    Task<ServiceResult<bool>> DeleteAsync(int id);
}

public interface ISubjectService
{
    Task<ServiceResult<PagedResult<List<SubjectBM>>>> GetAllAsync(QueryParams queryParams);
    Task<ServiceResult<SubjectBM>> GetByIdAsync(int id);
    Task<ServiceResult<SubjectBM>> CreateAsync(SubjectBM request);
    Task<ServiceResult<SubjectBM>> UpdateAsync(int id, SubjectBM request);
    Task<ServiceResult<bool>> DeleteAsync(int id);
}
