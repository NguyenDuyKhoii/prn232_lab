using Microsoft.EntityFrameworkCore;
using PRN232.LMS.Repositories.Entities;
using PRN232.LMS.Repositories.DbContexts;
using PRN232.LMS.Repositories.Interfaces;
using System.Linq.Expressions;

namespace PRN232.LMS.Repositories.Implementations;

public class Repository<T> : IRepository<T> where T : class
{
    protected readonly LmsDbContext _context;
    protected readonly DbSet<T> _dbSet;

    public Repository(LmsDbContext context)
    {
        _context = context;
        _dbSet = context.Set<T>();
    }

    public virtual async Task<T?> GetByIdAsync(int id)
    {
        return await _dbSet.FindAsync(id);
    }

    public virtual async Task<IEnumerable<T>> GetAllAsync()
    {
        return await _dbSet.ToListAsync();
    }

    public async Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate)
    {
        return await _dbSet.Where(predicate).ToListAsync();
    }

    public virtual async Task<T> AddAsync(T entity)
    {
        await _dbSet.AddAsync(entity);
        await _context.SaveChangesAsync();
        return entity;
    }

    public virtual async Task UpdateAsync(T entity)
    {
        _dbSet.Update(entity);
        await _context.SaveChangesAsync();
    }

    public virtual async Task DeleteAsync(int id)
    {
        var entity = await GetByIdAsync(id);
        if (entity != null)
        {
            _dbSet.Remove(entity);
            await _context.SaveChangesAsync();
        }
    }

    public async Task<bool> ExistsAsync(int id)
    {
        return await _dbSet.FindAsync(id) != null;
    }
}

public class SemesterRepository : Repository<Semester>, ISemesterRepository
{
    public SemesterRepository(LmsDbContext context) : base(context) { }

    public async Task<Semester?> GetByIdWithCoursesAsync(int id)
    {
        return await _dbSet.Include(s => s.Courses).FirstOrDefaultAsync(s => s.SemesterId == id);
    }

    public IQueryable<Semester> GetAllQueryable()
    {
        return _dbSet;
    }
}

public class CourseRepository : Repository<Course>, ICourseRepository
{
    public CourseRepository(LmsDbContext context) : base(context) { }

    public async Task<Course?> GetByIdWithSemesterAsync(int id)
    {
        return await _dbSet.Include(c => c.Semester).FirstOrDefaultAsync(c => c.CourseId == id);
    }

    public async Task<Course?> GetByIdWithEnrollmentsAsync(int id)
    {
        return await _dbSet
            .Include(c => c.Enrollments)
            .ThenInclude(e => e.Student)
            .FirstOrDefaultAsync(c => c.CourseId == id);
    }

    public IQueryable<Course> GetAllQueryable()
    {
        return _dbSet;
    }
}

public class StudentRepository : Repository<Student>, IStudentRepository
{
    public StudentRepository(LmsDbContext context) : base(context) { }

    public async Task<Student?> GetByIdWithEnrollmentsAsync(int id)
    {
        return await _dbSet
            .Include(s => s.Enrollments)
            .ThenInclude(e => e.Course)
            .FirstOrDefaultAsync(s => s.StudentId == id);
    }

    public async Task<bool> ExistsByEmailAsync(string email, int? excludeId = null)
    {
        return await _dbSet.AnyAsync(s => s.Email == email && (excludeId == null || s.StudentId != excludeId));
    }

    public IQueryable<Student> GetAllQueryable()
    {
        return _dbSet;
    }
}

public class EnrollmentRepository : Repository<Enrollment>, IEnrollmentRepository
{
    public EnrollmentRepository(LmsDbContext context) : base(context) { }

    public async Task<Enrollment?> GetByIdWithDetailsAsync(int id)
    {
        return await _dbSet
            .Include(e => e.Student)
            .Include(e => e.Course)
            .ThenInclude(c => c!.Semester)
            .FirstOrDefaultAsync(e => e.EnrollmentId == id);
    }

    public async Task<bool> ExistsByStudentAndCourseAsync(int studentId, int courseId, int? excludeId = null)
    {
        return await _dbSet.AnyAsync(e =>
            e.StudentId == studentId &&
            e.CourseId == courseId &&
            (excludeId == null || e.EnrollmentId != excludeId));
    }

    public IQueryable<Enrollment> GetAllQueryable()
    {
        return _dbSet;
    }
}

public class SubjectRepository : Repository<Subject>, ISubjectRepository
{
    public SubjectRepository(LmsDbContext context) : base(context) { }

    public override async Task<Subject?> GetByIdAsync(int id)
    {
        return await _dbSet.FindAsync(id);
    }

    public async Task<bool> ExistsByCodeAsync(string code, int? excludeId = null)
    {
        return await _dbSet.AnyAsync(s => s.SubjectCode == code && (excludeId == null || s.SubjectId != excludeId));
    }

    public IQueryable<Subject> GetAllQueryable()
    {
        return _dbSet;
    }
}
