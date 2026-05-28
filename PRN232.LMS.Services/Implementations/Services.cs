using AutoMapper;
using Microsoft.EntityFrameworkCore;
using PRN232.LMS.Services.BusinessModels;
using PRN232.LMS.Services.Common;
using PRN232.LMS.Repositories.Entities;
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

    public async Task<ServiceResult<Common.PagedResult<List<SemesterBM>>>> GetAllAsync(QueryParams queryParams)
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
        var totalItems = await query.CountAsync();
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

        var businessModels = _mapper.Map<List<SemesterBM>>(data);

        var pagedResult = new Common.PagedResult<List<SemesterBM>>
        {
            Data = businessModels,
            Page = queryParams.Page,
            PageSize = queryParams.Size,
            TotalItems = totalItems,
            TotalPages = totalPages
        };

        return ServiceResult<Common.PagedResult<List<SemesterBM>>>.Ok(pagedResult);
    }

    public async Task<ServiceResult<SemesterBM>> GetByIdAsync(int id, string? expand = null)
    {
        Semester? entity = null;

        if (!string.IsNullOrWhiteSpace(expand))
        {
            var expands = expand.Split(',').Select(e => e.Trim().ToLower());
            if (expands.Contains("courses"))
                entity = await _repository.GetByIdWithCoursesAsync(id);
        }

        entity ??= await _repository.GetByIdAsync(id);

        if (entity == null)
            return ServiceResult<SemesterBM>.NotFound("Semester not found");

        var businessModel = _mapper.Map<SemesterBM>(entity);
        return ServiceResult<SemesterBM>.Ok(businessModel);
    }

    public async Task<ServiceResult<SemesterBM>> CreateAsync(SemesterBM request)
    {
        var entity = _mapper.Map<Semester>(request);
        await _repository.AddAsync(entity);

        var resultBM = _mapper.Map<SemesterBM>(entity);
        return ServiceResult<SemesterBM>.Ok(resultBM, "Semester created successfully");
    }

    public async Task<ServiceResult<SemesterBM>> UpdateAsync(int id, SemesterBM request)
    {
        var entity = await _repository.GetByIdAsync(id);
        if (entity == null)
            return ServiceResult<SemesterBM>.NotFound("Semester not found");

        entity.SemesterName = request.SemesterName;
        entity.StartDate = request.StartDate;
        entity.EndDate = request.EndDate;

        await _repository.UpdateAsync(entity);

        var resultBM = _mapper.Map<SemesterBM>(entity);
        return ServiceResult<SemesterBM>.Ok(resultBM, "Semester updated successfully");
    }

    public async Task<ServiceResult<bool>> DeleteAsync(int id)
    {
        var entity = await _repository.GetByIdAsync(id);
        if (entity == null)
            return ServiceResult<bool>.NotFound("Semester not found");

        await _repository.DeleteAsync(id);
        return ServiceResult<bool>.Ok(true, "Semester deleted successfully");
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

    public async Task<ServiceResult<Common.PagedResult<List<CourseBM>>>> GetAllAsync(QueryParams queryParams)
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

        var totalItems = await query.CountAsync();
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

        var businessModels = _mapper.Map<List<CourseBM>>(data);

        var pagedResult = new Common.PagedResult<List<CourseBM>>
        {
            Data = businessModels,
            Page = queryParams.Page,
            PageSize = queryParams.Size,
            TotalItems = totalItems,
            TotalPages = totalPages
        };

        return ServiceResult<Common.PagedResult<List<CourseBM>>>.Ok(pagedResult);
    }

    public async Task<ServiceResult<CourseBM>> GetByIdAsync(int id, string? expand = null)
    {
        Course? entity = null;

        if (!string.IsNullOrWhiteSpace(expand))
        {
            var expands = expand.Split(',').Select(e => e.Trim().ToLower());
            if (expands.Contains("enrollments"))
                entity = await _repository.GetByIdWithEnrollmentsAsync(id);
            else if (expands.Contains("semester"))
                entity = await _repository.GetByIdWithSemesterAsync(id);
        }

        entity ??= await _repository.GetByIdAsync(id);

        if (entity == null)
            return ServiceResult<CourseBM>.NotFound("Course not found");

        var businessModel = _mapper.Map<CourseBM>(entity);
        return ServiceResult<CourseBM>.Ok(businessModel);
    }

    public async Task<ServiceResult<CourseBM>> CreateAsync(CourseBM request)
    {
        var entity = _mapper.Map<Course>(request);
        await _repository.AddAsync(entity);

        var resultBM = _mapper.Map<CourseBM>(entity);
        return ServiceResult<CourseBM>.Ok(resultBM, "Course created successfully");
    }

    public async Task<ServiceResult<CourseBM>> UpdateAsync(int id, CourseBM request)
    {
        var entity = await _repository.GetByIdAsync(id);
        if (entity == null)
            return ServiceResult<CourseBM>.NotFound("Course not found");

        entity.CourseName = request.CourseName;
        entity.SemesterId = request.SemesterId;

        await _repository.UpdateAsync(entity);

        var resultBM = _mapper.Map<CourseBM>(entity);
        return ServiceResult<CourseBM>.Ok(resultBM, "Course updated successfully");
    }

    public async Task<ServiceResult<bool>> DeleteAsync(int id)
    {
        var entity = await _repository.GetByIdAsync(id);
        if (entity == null)
            return ServiceResult<bool>.NotFound("Course not found");

        await _repository.DeleteAsync(id);
        return ServiceResult<bool>.Ok(true, "Course deleted successfully");
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

    public async Task<ServiceResult<Common.PagedResult<List<StudentBM>>>> GetAllAsync(QueryParams queryParams)
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

        var totalItems = await query.CountAsync();
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

        var businessModels = _mapper.Map<List<StudentBM>>(data);

        var pagedResult = new Common.PagedResult<List<StudentBM>>
        {
            Data = businessModels,
            Page = queryParams.Page,
            PageSize = queryParams.Size,
            TotalItems = totalItems,
            TotalPages = totalPages
        };

        return ServiceResult<Common.PagedResult<List<StudentBM>>>.Ok(pagedResult);
    }

    public async Task<ServiceResult<StudentBM>> GetByIdAsync(int id, string? expand = null)
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
            return ServiceResult<StudentBM>.NotFound("Student not found");

        var businessModel = _mapper.Map<StudentBM>(entity);
        return ServiceResult<StudentBM>.Ok(businessModel);
    }

    public async Task<ServiceResult<StudentBM>> CreateAsync(StudentBM request)
    {
        if (await _repository.ExistsByEmailAsync(request.Email))
        {
            return ServiceResult<StudentBM>.BadRequest("Email already exists");
        }

        var entity = _mapper.Map<Student>(request);
        await _repository.AddAsync(entity);

        var resultBM = _mapper.Map<StudentBM>(entity);
        return ServiceResult<StudentBM>.Ok(resultBM, "Student created successfully");
    }

    public async Task<ServiceResult<StudentBM>> UpdateAsync(int id, StudentBM request)
    {
        var entity = await _repository.GetByIdAsync(id);
        if (entity == null)
            return ServiceResult<StudentBM>.NotFound("Student not found");

        if (await _repository.ExistsByEmailAsync(request.Email, id))
        {
            return ServiceResult<StudentBM>.BadRequest("Email already exists");
        }

        entity.FullName = request.FullName;
        entity.Email = request.Email;
        entity.DateOfBirth = request.DateOfBirth;

        await _repository.UpdateAsync(entity);

        var resultBM = _mapper.Map<StudentBM>(entity);
        return ServiceResult<StudentBM>.Ok(resultBM, "Student updated successfully");
    }

    public async Task<ServiceResult<bool>> DeleteAsync(int id)
    {
        var entity = await _repository.GetByIdAsync(id);
        if (entity == null)
            return ServiceResult<bool>.NotFound("Student not found");

        await _repository.DeleteAsync(id);
        return ServiceResult<bool>.Ok(true, "Student deleted successfully");
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

    public async Task<ServiceResult<Common.PagedResult<List<EnrollmentBM>>>> GetAllAsync(QueryParams queryParams)
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

        var totalItems = await query.CountAsync();
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

        var businessModels = _mapper.Map<List<EnrollmentBM>>(data);

        var pagedResult = new Common.PagedResult<List<EnrollmentBM>>
        {
            Data = businessModels,
            Page = queryParams.Page,
            PageSize = queryParams.Size,
            TotalItems = totalItems,
            TotalPages = totalPages
        };

        return ServiceResult<Common.PagedResult<List<EnrollmentBM>>>.Ok(pagedResult);
    }

    public async Task<ServiceResult<Common.PagedResult<List<EnrollmentBM>>>> GetByCourseIdAsync(int courseId, int page, int size)
    {
        var query = _repository.GetAllQueryable()
            .Include(e => e.Student)
            .Include(e => e.Course)
            .Where(e => e.CourseId == courseId);

        var totalItems = await query.CountAsync();
        var totalPages = (int)Math.Ceiling((double)totalItems / size);

        var data = await query
            .OrderBy(e => e.EnrollmentId)
            .Skip((page - 1) * size)
            .Take(size)
            .ToListAsync();

        var businessModels = _mapper.Map<List<EnrollmentBM>>(data);

        var pagedResult = new Common.PagedResult<List<EnrollmentBM>>
        {
            Data = businessModels,
            Page = page,
            PageSize = size,
            TotalItems = totalItems,
            TotalPages = totalPages
        };

        return ServiceResult<Common.PagedResult<List<EnrollmentBM>>>.Ok(pagedResult);
    }

    public async Task<ServiceResult<EnrollmentBM>> GetByIdAsync(int id, string? expand = null)
    {
        Enrollment? entity = null;

        if (!string.IsNullOrWhiteSpace(expand))
        {
            var expands = expand.Split(',').Select(e => e.Trim().ToLower());
            if (expands.Contains("student") || expands.Contains("course"))
                entity = await _repository.GetByIdWithDetailsAsync(id);
        }

        entity ??= await _repository.GetByIdAsync(id);

        if (entity == null)
            return ServiceResult<EnrollmentBM>.NotFound("Enrollment not found");

        var businessModel = _mapper.Map<EnrollmentBM>(entity);
        return ServiceResult<EnrollmentBM>.Ok(businessModel);
    }

    public async Task<ServiceResult<EnrollmentBM>> CreateAsync(EnrollmentBM request)
    {
        if (await _repository.ExistsByStudentAndCourseAsync(request.StudentId, request.CourseId))
        {
            return ServiceResult<EnrollmentBM>.BadRequest("Student already enrolled in this course");
        }

        var entity = _mapper.Map<Enrollment>(request);
        await _repository.AddAsync(entity);

        var resultBM = _mapper.Map<EnrollmentBM>(entity);
        return ServiceResult<EnrollmentBM>.Ok(resultBM, "Enrollment created successfully");
    }

    public async Task<ServiceResult<EnrollmentBM>> UpdateAsync(int id, EnrollmentBM request)
    {
        var entity = await _repository.GetByIdAsync(id);
        if (entity == null)
            return ServiceResult<EnrollmentBM>.NotFound("Enrollment not found");

        if (await _repository.ExistsByStudentAndCourseAsync(request.StudentId, request.CourseId, id))
        {
            return ServiceResult<EnrollmentBM>.BadRequest("Student already enrolled in this course");
        }

        entity.StudentId = request.StudentId;
        entity.CourseId = request.CourseId;
        entity.EnrollDate = request.EnrollDate;
        entity.Status = request.Status;

        await _repository.UpdateAsync(entity);

        var resultBM = _mapper.Map<EnrollmentBM>(entity);
        return ServiceResult<EnrollmentBM>.Ok(resultBM, "Enrollment updated successfully");
    }

    public async Task<ServiceResult<bool>> DeleteAsync(int id)
    {
        var entity = await _repository.GetByIdAsync(id);
        if (entity == null)
            return ServiceResult<bool>.NotFound("Enrollment not found");

        await _repository.DeleteAsync(id);
        return ServiceResult<bool>.Ok(true, "Enrollment deleted successfully");
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

    public async Task<ServiceResult<Common.PagedResult<List<SubjectBM>>>> GetAllAsync(QueryParams queryParams)
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

        var totalItems = await query.CountAsync();
        var totalPages = (int)Math.Ceiling((double)totalItems / queryParams.Size);
        var data = await query
            .Skip((queryParams.Page - 1) * queryParams.Size)
            .Take(queryParams.Size)
            .ToListAsync();

        var businessModels = _mapper.Map<List<SubjectBM>>(data);

        var pagedResult = new Common.PagedResult<List<SubjectBM>>
        {
            Data = businessModels,
            Page = queryParams.Page,
            PageSize = queryParams.Size,
            TotalItems = totalItems,
            TotalPages = totalPages
        };

        return ServiceResult<Common.PagedResult<List<SubjectBM>>>.Ok(pagedResult);
    }

    public async Task<ServiceResult<SubjectBM>> GetByIdAsync(int id)
    {
        var entity = await _repository.GetByIdAsync(id);
        if (entity == null)
            return ServiceResult<SubjectBM>.NotFound("Subject not found");

        var businessModel = _mapper.Map<SubjectBM>(entity);
        return ServiceResult<SubjectBM>.Ok(businessModel);
    }

    public async Task<ServiceResult<SubjectBM>> CreateAsync(SubjectBM request)
    {
        if (await _repository.ExistsByCodeAsync(request.SubjectCode))
        {
            return ServiceResult<SubjectBM>.BadRequest("Subject code already exists");
        }

        var entity = _mapper.Map<Subject>(request);
        await _repository.AddAsync(entity);

        var resultBM = _mapper.Map<SubjectBM>(entity);
        return ServiceResult<SubjectBM>.Ok(resultBM, "Subject created successfully");
    }

    public async Task<ServiceResult<SubjectBM>> UpdateAsync(int id, SubjectBM request)
    {
        var entity = await _repository.GetByIdAsync(id);
        if (entity == null)
            return ServiceResult<SubjectBM>.NotFound("Subject not found");

        if (await _repository.ExistsByCodeAsync(request.SubjectCode, id))
        {
            return ServiceResult<SubjectBM>.BadRequest("Subject code already exists");
        }

        entity.SubjectCode = request.SubjectCode;
        entity.SubjectName = request.SubjectName;
        entity.Credit = request.Credit;

        await _repository.UpdateAsync(entity);

        var resultBM = _mapper.Map<SubjectBM>(entity);
        return ServiceResult<SubjectBM>.Ok(resultBM, "Subject updated successfully");
    }

    public async Task<ServiceResult<bool>> DeleteAsync(int id)
    {
        var entity = await _repository.GetByIdAsync(id);
        if (entity == null)
            return ServiceResult<bool>.NotFound("Subject not found");

        await _repository.DeleteAsync(id);
        return ServiceResult<bool>.Ok(true, "Subject deleted successfully");
    }
}
