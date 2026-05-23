using AutoMapper;
using Microsoft.EntityFrameworkCore;
using PRN232.LMS.Services.BusinessModels;
using PRN232.LMS.Services.Common;
using PRN232.LMS.Repositories.Entities;
using PRN232.LMS.Services.Requests;
using PRN232.LMS.Services.Responses;
using PRN232.LMS.Repositories.Interfaces;
using PRN232.LMS.Services.Interfaces;
using System.Linq.Dynamic.Core;
using System.Reflection;

namespace PRN232.LMS.Services.Implementations;

// Shared helper for sort parsing and field selection
internal static class QueryHelper
{
    /// <summary>
    /// Parse sort expression: "fullName,-dateOfBirth" → "fullName asc, dateOfBirth desc"
    /// </summary>
    public static string ParseSortExpression(string sort)
    {
        var parts = sort.Split(',');
        var expressions = new List<string>();

        foreach (var part in parts)
        {
            var trimmed = part.Trim();
            if (string.IsNullOrEmpty(trimmed)) continue;

            if (trimmed.StartsWith("-"))
            {
                expressions.Add(trimmed.Substring(1) + " desc");
            }
            else
            {
                expressions.Add(trimmed + " asc");
            }
        }

        return string.Join(", ", expressions);
    }

    /// <summary>
    /// Select specific fields from a list of objects (case-insensitive field matching)
    /// </summary>
    public static List<T> SelectFields<T>(List<T> data, string fields) where T : class, new()
    {
        var fieldList = fields.Split(',').Select(f => f.Trim()).ToList();
        var result = new List<T>();

        foreach (var item in data)
        {
            var newItem = new T();
            foreach (var field in fieldList)
            {
                var property = typeof(T).GetProperty(field, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);
                if (property != null && property.CanWrite)
                {
                    var value = property.GetValue(item);
                    property.SetValue(newItem, value);
                }
            }
            result.Add(newItem);
        }

        return result;
    }
}

public class SemesterService : ISemesterService
{
    private readonly ISemesterRepository _repository;
    private readonly IMapper _mapper;

    public SemesterService(ISemesterRepository repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<PaginatedResponse<List<SemesterResponse>>> GetAllAsync(QueryParams queryParams)
    {
        var query = _repository.GetAllQueryable();

        // Search
        if (!string.IsNullOrWhiteSpace(queryParams.Search))
        {
            query = query.Where(s => s.SemesterName.Contains(queryParams.Search));
        }

        // Sort
        if (!string.IsNullOrWhiteSpace(queryParams.Sort))
        {
            query = query.OrderBy(QueryHelper.ParseSortExpression(queryParams.Sort));
        }
        else
        {
            query = query.OrderBy(s => s.StartDate);
        }

        // Pagination
        var totalItems = query.Count();
        var totalPages = (int)Math.Ceiling((double)totalItems / queryParams.Size);

        // Expansion
        if (!string.IsNullOrWhiteSpace(queryParams.Expand))
        {
            var expands = queryParams.Expand.Split(',').Select(e => e.Trim().ToLower());
            if (expands.Contains("courses"))
                query = query.Include(s => s.Courses);
        }

        var data = await query
            .Skip((queryParams.Page - 1) * queryParams.Size)
            .Take(queryParams.Size)
            .ToListAsync();

        var response = _mapper.Map<List<SemesterResponse>>(data);

        // Field selection
        if (!string.IsNullOrWhiteSpace(queryParams.Fields))
        {
            response = QueryHelper.SelectFields(response, queryParams.Fields);
        }

        return PaginatedResponse<List<SemesterResponse>>.Ok(response,
            new PaginationMetadata { Page = queryParams.Page, PageSize = queryParams.Size, TotalItems = totalItems, TotalPages = totalPages });
    }

    public async Task<ApiResponse<SemesterResponse>> GetByIdAsync(int id)
    {
        var entity = await _repository.GetByIdWithCoursesAsync(id);
        if (entity == null)
            return ApiResponse<SemesterResponse>.Fail("Semester not found", new List<string> { "404" });

        var response = _mapper.Map<SemesterResponse>(entity);
        return ApiResponse<SemesterResponse>.Ok(response);
    }

    public async Task<ApiResponse<SemesterResponse>> CreateAsync(CreateSemesterRequest request)
    {
        var entity = _mapper.Map<Semester>(request);
        await _repository.AddAsync(entity);
        var response = _mapper.Map<SemesterResponse>(entity);
        return ApiResponse<SemesterResponse>.Ok(response, "Semester created successfully");
    }

    public async Task<ApiResponse<SemesterResponse>> UpdateAsync(int id, UpdateSemesterRequest request)
    {
        var entity = await _repository.GetByIdAsync(id);
        if (entity == null)
            return ApiResponse<SemesterResponse>.Fail("Semester not found", new List<string> { "404" });

        entity.SemesterName = request.SemesterName;
        entity.StartDate = request.StartDate;
        entity.EndDate = request.EndDate;

        await _repository.UpdateAsync(entity);
        var response = _mapper.Map<SemesterResponse>(entity);
        return ApiResponse<SemesterResponse>.Ok(response, "Semester updated successfully");
    }

    public async Task<ApiResponse<bool>> DeleteAsync(int id)
    {
        var entity = await _repository.GetByIdAsync(id);
        if (entity == null)
            return ApiResponse<bool>.Fail("Semester not found", new List<string> { "404" });

        await _repository.DeleteAsync(id);
        return ApiResponse<bool>.Ok(true, "Semester deleted successfully");
    }
}

public class CourseService : ICourseService
{
    private readonly ICourseRepository _repository;
    private readonly IMapper _mapper;

    public CourseService(ICourseRepository repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<PaginatedResponse<List<CourseResponse>>> GetAllAsync(QueryParams queryParams)
    {
        var query = _repository.GetAllQueryable();

        // Search
        if (!string.IsNullOrWhiteSpace(queryParams.Search))
        {
            query = query.Where(c => c.CourseName.Contains(queryParams.Search));
        }

        // Sort
        if (!string.IsNullOrWhiteSpace(queryParams.Sort))
        {
            query = query.OrderBy(QueryHelper.ParseSortExpression(queryParams.Sort));
        }
        else
        {
            query = query.OrderBy(c => c.CourseId);
        }

        var totalItems = query.Count();
        var totalPages = (int)Math.Ceiling((double)totalItems / queryParams.Size);

        // Expansion
        if (!string.IsNullOrWhiteSpace(queryParams.Expand))
        {
            var expands = queryParams.Expand.Split(',').Select(e => e.Trim().ToLower());
            if (expands.Contains("semester"))
                query = query.Include(c => c.Semester);
            if (expands.Contains("enrollments"))
                query = query.Include(c => c.Enrollments).ThenInclude(e => e.Student);
        }
        else
        {
            query = query.Include(c => c.Semester);
        }

        var data = await query
            .Skip((queryParams.Page - 1) * queryParams.Size)
            .Take(queryParams.Size)
            .ToListAsync();

        var response = _mapper.Map<List<CourseResponse>>(data);

        if (!string.IsNullOrWhiteSpace(queryParams.Fields))
        {
            response = QueryHelper.SelectFields(response, queryParams.Fields);
        }

        return PaginatedResponse<List<CourseResponse>>.Ok(response,
            new PaginationMetadata { Page = queryParams.Page, PageSize = queryParams.Size, TotalItems = totalItems, TotalPages = totalPages });
    }

    public async Task<ApiResponse<CourseResponse>> GetByIdAsync(int id, string? expand = null)
    {
        Course? entity = null;

        if (expand?.Contains("semester") == true)
        {
            entity = await _repository.GetByIdWithSemesterAsync(id);
        }
        else
        {
            entity = await _repository.GetByIdAsync(id);
        }

        if (entity == null)
            return ApiResponse<CourseResponse>.Fail("Course not found", new List<string> { "404" });

        var response = _mapper.Map<CourseResponse>(entity);
        return ApiResponse<CourseResponse>.Ok(response);
    }

    public async Task<ApiResponse<CourseResponse>> CreateAsync(CreateCourseRequest request)
    {
        var entity = _mapper.Map<Course>(request);
        await _repository.AddAsync(entity);
        var response = _mapper.Map<CourseResponse>(entity);
        return ApiResponse<CourseResponse>.Ok(response, "Course created successfully");
    }

    public async Task<ApiResponse<CourseResponse>> UpdateAsync(int id, UpdateCourseRequest request)
    {
        var entity = await _repository.GetByIdAsync(id);
        if (entity == null)
            return ApiResponse<CourseResponse>.Fail("Course not found", new List<string> { "404" });

        entity.CourseName = request.CourseName;
        entity.SemesterId = request.SemesterId;

        await _repository.UpdateAsync(entity);
        var response = _mapper.Map<CourseResponse>(entity);
        return ApiResponse<CourseResponse>.Ok(response, "Course updated successfully");
    }

    public async Task<ApiResponse<bool>> DeleteAsync(int id)
    {
        var entity = await _repository.GetByIdAsync(id);
        if (entity == null)
            return ApiResponse<bool>.Fail("Course not found", new List<string> { "404" });

        await _repository.DeleteAsync(id);
        return ApiResponse<bool>.Ok(true, "Course deleted successfully");
    }
}

public class StudentService : IStudentService
{
    private readonly IStudentRepository _repository;
    private readonly IMapper _mapper;

    public StudentService(IStudentRepository repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<PaginatedResponse<List<StudentResponse>>> GetAllAsync(QueryParams queryParams)
    {
        var query = _repository.GetAllQueryable();

        if (!string.IsNullOrWhiteSpace(queryParams.Search))
        {
            query = query.Where(s => s.FullName.Contains(queryParams.Search) || s.Email.Contains(queryParams.Search));
        }

        if (!string.IsNullOrWhiteSpace(queryParams.Sort))
        {
            query = query.OrderBy(QueryHelper.ParseSortExpression(queryParams.Sort));
        }
        else
        {
            query = query.OrderBy(s => s.StudentId);
        }

        var totalItems = query.Count();
        var totalPages = (int)Math.Ceiling((double)totalItems / queryParams.Size);

        // Expansion
        if (!string.IsNullOrWhiteSpace(queryParams.Expand))
        {
            var expands = queryParams.Expand.Split(',').Select(e => e.Trim().ToLower());
            if (expands.Contains("enrollments"))
                query = query.Include(s => s.Enrollments).ThenInclude(e => e.Course);
        }

        var data = await query
            .Skip((queryParams.Page - 1) * queryParams.Size)
            .Take(queryParams.Size)
            .ToListAsync();

        var response = _mapper.Map<List<StudentResponse>>(data);

        if (!string.IsNullOrWhiteSpace(queryParams.Fields))
        {
            response = QueryHelper.SelectFields(response, queryParams.Fields);
        }

        return PaginatedResponse<List<StudentResponse>>.Ok(response,
            new PaginationMetadata { Page = queryParams.Page, PageSize = queryParams.Size, TotalItems = totalItems, TotalPages = totalPages });
    }

    public async Task<ApiResponse<StudentResponse>> GetByIdAsync(int id, string? expand = null)
    {
        Student? entity = null;

        if (expand?.Contains("enrollments") == true)
        {
            entity = await _repository.GetByIdWithEnrollmentsAsync(id);
        }
        else
        {
            entity = await _repository.GetByIdAsync(id);
        }

        if (entity == null)
            return ApiResponse<StudentResponse>.Fail("Student not found", new List<string> { "404" });

        var response = _mapper.Map<StudentResponse>(entity);
        return ApiResponse<StudentResponse>.Ok(response);
    }

    public async Task<ApiResponse<StudentResponse>> CreateAsync(CreateStudentRequest request)
    {
        if (await _repository.ExistsByEmailAsync(request.Email))
        {
            return ApiResponse<StudentResponse>.Fail("Email already exists", new List<string> { "400" });
        }

        var entity = _mapper.Map<Student>(request);
        await _repository.AddAsync(entity);
        var response = _mapper.Map<StudentResponse>(entity);
        return ApiResponse<StudentResponse>.Ok(response, "Student created successfully");
    }

    public async Task<ApiResponse<StudentResponse>> UpdateAsync(int id, UpdateStudentRequest request)
    {
        var entity = await _repository.GetByIdAsync(id);
        if (entity == null)
            return ApiResponse<StudentResponse>.Fail("Student not found", new List<string> { "404" });

        if (await _repository.ExistsByEmailAsync(request.Email, id))
        {
            return ApiResponse<StudentResponse>.Fail("Email already exists", new List<string> { "400" });
        }

        entity.FullName = request.FullName;
        entity.Email = request.Email;
        entity.DateOfBirth = request.DateOfBirth;

        await _repository.UpdateAsync(entity);
        var response = _mapper.Map<StudentResponse>(entity);
        return ApiResponse<StudentResponse>.Ok(response, "Student updated successfully");
    }

    public async Task<ApiResponse<bool>> DeleteAsync(int id)
    {
        var entity = await _repository.GetByIdAsync(id);
        if (entity == null)
            return ApiResponse<bool>.Fail("Student not found", new List<string> { "404" });

        await _repository.DeleteAsync(id);
        return ApiResponse<bool>.Ok(true, "Student deleted successfully");
    }
}

public class EnrollmentService : IEnrollmentService
{
    private readonly IEnrollmentRepository _repository;
    private readonly IMapper _mapper;

    public EnrollmentService(IEnrollmentRepository repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<PaginatedResponse<List<EnrollmentResponse>>> GetAllAsync(QueryParams queryParams)
    {
        var query = _repository.GetAllQueryable();

        if (!string.IsNullOrWhiteSpace(queryParams.Search))
        {
            query = query.Where(e => e.Status.Contains(queryParams.Search) ||
                                    e.Student.FullName.Contains(queryParams.Search) ||
                                    e.Course.CourseName.Contains(queryParams.Search));
        }

        if (!string.IsNullOrWhiteSpace(queryParams.Sort))
        {
            query = query.OrderBy(QueryHelper.ParseSortExpression(queryParams.Sort));
        }
        else
        {
            query = query.OrderBy(e => e.EnrollmentId);
        }

        var totalItems = query.Count();
        var totalPages = (int)Math.Ceiling((double)totalItems / queryParams.Size);

        // Expansion
        if (!string.IsNullOrWhiteSpace(queryParams.Expand))
        {
            var expands = queryParams.Expand.Split(',').Select(e => e.Trim().ToLower());
            if (expands.Contains("student"))
                query = query.Include(e => e.Student);
            if (expands.Contains("course"))
                query = query.Include(e => e.Course);
        }

        var data = await query
            .Skip((queryParams.Page - 1) * queryParams.Size)
            .Take(queryParams.Size)
            .ToListAsync();

        var response = _mapper.Map<List<EnrollmentResponse>>(data);

        if (!string.IsNullOrWhiteSpace(queryParams.Fields))
        {
            response = QueryHelper.SelectFields(response, queryParams.Fields);
        }

        return PaginatedResponse<List<EnrollmentResponse>>.Ok(response,
            new PaginationMetadata { Page = queryParams.Page, PageSize = queryParams.Size, TotalItems = totalItems, TotalPages = totalPages });
    }

    public async Task<ApiResponse<EnrollmentResponse>> GetByIdAsync(int id, string? expand = null)
    {
        var entity = await _repository.GetByIdWithDetailsAsync(id);
        if (entity == null)
            return ApiResponse<EnrollmentResponse>.Fail("Enrollment not found", new List<string> { "404" });

        var response = _mapper.Map<EnrollmentResponse>(entity);
        return ApiResponse<EnrollmentResponse>.Ok(response);
    }

    public async Task<ApiResponse<EnrollmentResponse>> CreateAsync(CreateEnrollmentRequest request)
    {
        if (await _repository.ExistsByStudentAndCourseAsync(request.StudentId, request.CourseId))
        {
            return ApiResponse<EnrollmentResponse>.Fail("Student already enrolled in this course", new List<string> { "400" });
        }

        var entity = _mapper.Map<Enrollment>(request);
        await _repository.AddAsync(entity);
        var response = _mapper.Map<EnrollmentResponse>(entity);
        return ApiResponse<EnrollmentResponse>.Ok(response, "Enrollment created successfully");
    }

    public async Task<ApiResponse<EnrollmentResponse>> UpdateAsync(int id, UpdateEnrollmentRequest request)
    {
        var entity = await _repository.GetByIdAsync(id);
        if (entity == null)
            return ApiResponse<EnrollmentResponse>.Fail("Enrollment not found", new List<string> { "404" });

        if (await _repository.ExistsByStudentAndCourseAsync(request.StudentId, request.CourseId, id))
        {
            return ApiResponse<EnrollmentResponse>.Fail("Student already enrolled in this course", new List<string> { "400" });
        }

        entity.StudentId = request.StudentId;
        entity.CourseId = request.CourseId;
        entity.EnrollDate = request.EnrollDate;
        entity.Status = request.Status;

        await _repository.UpdateAsync(entity);
        var response = _mapper.Map<EnrollmentResponse>(entity);
        return ApiResponse<EnrollmentResponse>.Ok(response, "Enrollment updated successfully");
    }

    public async Task<ApiResponse<bool>> DeleteAsync(int id)
    {
        var entity = await _repository.GetByIdAsync(id);
        if (entity == null)
            return ApiResponse<bool>.Fail("Enrollment not found", new List<string> { "404" });

        await _repository.DeleteAsync(id);
        return ApiResponse<bool>.Ok(true, "Enrollment deleted successfully");
    }
}

public class SubjectService : ISubjectService
{
    private readonly ISubjectRepository _repository;
    private readonly IMapper _mapper;

    public SubjectService(ISubjectRepository repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<PaginatedResponse<List<SubjectResponse>>> GetAllAsync(QueryParams queryParams)
    {
        var query = _repository.GetAllQueryable();

        if (!string.IsNullOrWhiteSpace(queryParams.Search))
        {
            query = query.Where(s => s.SubjectName.Contains(queryParams.Search) ||
                                    s.SubjectCode.Contains(queryParams.Search));
        }

        if (!string.IsNullOrWhiteSpace(queryParams.Sort))
        {
            query = query.OrderBy(QueryHelper.ParseSortExpression(queryParams.Sort));
        }
        else
        {
            query = query.OrderBy(s => s.SubjectCode);
        }

        var totalItems = query.Count();
        var totalPages = (int)Math.Ceiling((double)totalItems / queryParams.Size);
        var data = await query
            .Skip((queryParams.Page - 1) * queryParams.Size)
            .Take(queryParams.Size)
            .ToListAsync();

        var response = _mapper.Map<List<SubjectResponse>>(data);

        if (!string.IsNullOrWhiteSpace(queryParams.Fields))
        {
            response = QueryHelper.SelectFields(response, queryParams.Fields);
        }

        return PaginatedResponse<List<SubjectResponse>>.Ok(response,
            new PaginationMetadata { Page = queryParams.Page, PageSize = queryParams.Size, TotalItems = totalItems, TotalPages = totalPages });
    }

    public async Task<ApiResponse<SubjectResponse>> GetByIdAsync(int id)
    {
        var entity = await _repository.GetByIdAsync(id);
        if (entity == null)
            return ApiResponse<SubjectResponse>.Fail("Subject not found", new List<string> { "404" });

        var response = _mapper.Map<SubjectResponse>(entity);
        return ApiResponse<SubjectResponse>.Ok(response);
    }

    public async Task<ApiResponse<SubjectResponse>> CreateAsync(CreateSubjectRequest request)
    {
        if (await _repository.ExistsByCodeAsync(request.SubjectCode))
        {
            return ApiResponse<SubjectResponse>.Fail("Subject code already exists", new List<string> { "400" });
        }

        var entity = _mapper.Map<Subject>(request);
        await _repository.AddAsync(entity);
        var response = _mapper.Map<SubjectResponse>(entity);
        return ApiResponse<SubjectResponse>.Ok(response, "Subject created successfully");
    }

    public async Task<ApiResponse<SubjectResponse>> UpdateAsync(int id, UpdateSubjectRequest request)
    {
        var entity = await _repository.GetByIdAsync(id);
        if (entity == null)
            return ApiResponse<SubjectResponse>.Fail("Subject not found", new List<string> { "404" });

        if (await _repository.ExistsByCodeAsync(request.SubjectCode, id))
        {
            return ApiResponse<SubjectResponse>.Fail("Subject code already exists", new List<string> { "400" });
        }

        entity.SubjectCode = request.SubjectCode;
        entity.SubjectName = request.SubjectName;
        entity.Credit = request.Credit;

        await _repository.UpdateAsync(entity);
        var response = _mapper.Map<SubjectResponse>(entity);
        return ApiResponse<SubjectResponse>.Ok(response, "Subject updated successfully");
    }

    public async Task<ApiResponse<bool>> DeleteAsync(int id)
    {
        var entity = await _repository.GetByIdAsync(id);
        if (entity == null)
            return ApiResponse<bool>.Fail("Subject not found", new List<string> { "404" });

        await _repository.DeleteAsync(id);
        return ApiResponse<bool>.Ok(true, "Subject deleted successfully");
    }
}
