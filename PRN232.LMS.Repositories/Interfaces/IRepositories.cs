using PRN232.LMS.Repositories.Entities;
using System.Linq.Expressions;

namespace PRN232.LMS.Repositories.Interfaces;

public interface IRepository<T> where T : class
{
    Task<T?> GetByIdAsync(int id);
    Task<IEnumerable<T>> GetAllAsync();
    Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate);
    Task<T> AddAsync(T entity);
    Task UpdateAsync(T entity);
    Task DeleteAsync(int id);
    Task<bool> ExistsAsync(int id);
}

public interface ISemesterRepository : IRepository<Semester>
{
    Task<Semester?> GetByIdWithCoursesAsync(int id);
    IQueryable<Semester> GetAllQueryable();
}

public interface ICourseRepository : IRepository<Course>
{
    Task<Course?> GetByIdWithSemesterAsync(int id);
    Task<Course?> GetByIdWithEnrollmentsAsync(int id);
    IQueryable<Course> GetAllQueryable();
}

public interface IStudentRepository : IRepository<Student>
{
    Task<Student?> GetByIdWithEnrollmentsAsync(int id);
    Task<bool> ExistsByEmailAsync(string email, int? excludeId = null);
    IQueryable<Student> GetAllQueryable();
}

public interface IEnrollmentRepository : IRepository<Enrollment>
{
    Task<Enrollment?> GetByIdWithDetailsAsync(int id);
    Task<bool> ExistsByStudentAndCourseAsync(int studentId, int courseId, int? excludeId = null);
    IQueryable<Enrollment> GetAllQueryable();
}

public interface ISubjectRepository : IRepository<Subject>
{
    Task<bool> ExistsByCodeAsync(string code, int? excludeId = null);
    IQueryable<Subject> GetAllQueryable();
}
