using Microsoft.EntityFrameworkCore;
using PRN232.LMS.Repositories.Entities;

namespace PRN232.LMS.Repositories.DbContexts;

public class LmsDbContext : DbContext
{
    public LmsDbContext(DbContextOptions<LmsDbContext> options) : base(options) { }

    public DbSet<Semester> Semesters => Set<Semester>();
    public DbSet<Course> Courses => Set<Course>();
    public DbSet<Student> Students => Set<Student>();
    public DbSet<Enrollment> Enrollments => Set<Enrollment>();
    public DbSet<Subject> Subjects => Set<Subject>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Semester
        modelBuilder.Entity<Semester>(entity =>
        {
            entity.HasKey(e => e.SemesterId);
            entity.Property(e => e.SemesterName).HasMaxLength(100).IsRequired();
            entity.Property(e => e.StartDate).IsRequired();
            entity.Property(e => e.EndDate).IsRequired();
        });

        // Course
        modelBuilder.Entity<Course>(entity =>
        {
            entity.HasKey(e => e.CourseId);
            entity.Property(e => e.CourseName).HasMaxLength(100).IsRequired();
            entity.HasOne(e => e.Semester)
                .WithMany(s => s.Courses)
                .HasForeignKey(e => e.SemesterId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // Student
        modelBuilder.Entity<Student>(entity =>
        {
            entity.HasKey(e => e.StudentId);
            entity.Property(e => e.FullName).HasMaxLength(100).IsRequired();
            entity.Property(e => e.Email).HasMaxLength(100).IsRequired();
            entity.HasIndex(e => e.Email).IsUnique();
        });

        // Enrollment
        modelBuilder.Entity<Enrollment>(entity =>
        {
            entity.HasKey(e => e.EnrollmentId);
            entity.Property(e => e.Status).HasMaxLength(20).IsRequired();
            entity.Property(e => e.EnrollDate).IsRequired();
            entity.HasOne(e => e.Student)
                .WithMany(s => s.Enrollments)
                .HasForeignKey(e => e.StudentId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(e => e.Course)
                .WithMany(c => c.Enrollments)
                .HasForeignKey(e => e.CourseId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasIndex(e => new { e.StudentId, e.CourseId }).IsUnique();
        });

        // Subject
        modelBuilder.Entity<Subject>(entity =>
        {
            entity.HasKey(e => e.SubjectId);
            entity.Property(e => e.SubjectCode).HasMaxLength(20).IsRequired();
            entity.Property(e => e.SubjectName).HasMaxLength(100).IsRequired();
            entity.Property(e => e.Credit).IsRequired();
            entity.HasIndex(e => e.SubjectCode).IsUnique();
        });

        // Seed Data
        SeedData(modelBuilder);
    }

    private static void SeedData(ModelBuilder modelBuilder)
    {
        // Semesters (5 semesters)
        var semesters = new List<Semester>();
        for (int i = 1; i <= 5; i++)
        {
            var year = 2023 + i / 2;
            var season = (i % 2 == 1) ? "Spring" : "Fall";
            semesters.Add(new Semester
            {
                SemesterId = i,
                SemesterName = $"{season} {year}",
                StartDate = new DateTime(year, i % 2 == 1 ? 1 : 8, 1),
                EndDate = new DateTime(year, i % 2 == 1 ? 5 : 12, 31)
            });
        }
        modelBuilder.Entity<Semester>().HasData(semesters);

        // Subjects (10 subjects)
        var subjects = new[]
        {
            new Subject { SubjectId = 1, SubjectCode = "PRN232", SubjectName = "Web Application Development", Credit = 3 },
            new Subject { SubjectId = 2, SubjectCode = "PRN231", SubjectName = "Database Management", Credit = 3 },
            new Subject { SubjectId = 3, SubjectCode = "PRN332", SubjectName = "Mobile Application Development", Credit = 3 },
            new Subject { SubjectId = 4, SubjectCode = "MTH101", SubjectName = "Calculus I", Credit = 4 },
            new Subject { SubjectId = 5, SubjectCode = "MTH102", SubjectName = "Linear Algebra", Credit = 3 },
            new Subject { SubjectId = 6, SubjectCode = "PHY101", SubjectName = "Physics I", Credit = 4 },
            new Subject { SubjectId = 7, SubjectCode = "ENG101", SubjectName = "English Communication", Credit = 3 },
            new Subject { SubjectId = 8, SubjectCode = "CS201", SubjectName = "Data Structures", Credit = 3 },
            new Subject { SubjectId = 9, SubjectCode = "CS301", SubjectName = "Algorithms", Credit = 3 },
            new Subject { SubjectId = 10, SubjectCode = "AI301", SubjectName = "Artificial Intelligence", Credit = 3 }
        };
        modelBuilder.Entity<Subject>().HasData(subjects);

        // Courses (20+ courses)
        var courses = new List<Course>();
        var courseNames = new[]
        {
            "Web Application Development - Class A", "Web Application Development - Class B",
            "Database Management - Class A", "Database Management - Class B",
            "Mobile Application Development - Class A",
            "Calculus I - Class A", "Calculus I - Class B",
            "Linear Algebra - Class A",
            "Physics I - Class A", "Physics I - Class B",
            "English Communication - Class A", "English Communication - Class B",
            "Data Structures - Class A", "Data Structures - Class B",
            "Algorithms - Class A",
            "Artificial Intelligence - Class A", "Artificial Intelligence - Class B",
            "Web Development Advanced", "Database Design", "Mobile App Advanced"
        };
        for (int i = 0; i < 20; i++)
        {
            courses.Add(new Course
            {
                CourseId = i + 1,
                CourseName = courseNames[i],
                SemesterId = (i % 5) + 1
            });
        }
        modelBuilder.Entity<Course>().HasData(courses);

        // Students (50 students)
        var vietnameseNames = new[]
        {
            "Nguyen Van A", "Tran Thi B", "Le Van C", "Pham Thi D", "Hoang Van E",
            "Nguyen Van F", "Tran Van G", "Le Thi H", "Pham Van I", "Hoang Thi J",
            "Nguyen Van K", "Tran Van L", "Le Thi M", "Pham Van N", "Hoang Van O",
            "Nguyen Thi P", "Tran Van Q", "Le Van R", "Pham Thi S", "Hoang Van T",
            "Nguyen Van U", "Tran Thi V", "Le Van W", "Pham Thi X", "Hoang Van Y",
            "Nguyen Van Z", "Tran Van AA", "Le Thi BB", "Pham Van CC", "Hoang Thi DD",
            "Nguyen Van EE", "Tran Van FF", "Le Thi GG", "Pham Van HH", "Hoang Van II",
            "Nguyen Thi JJ", "Tran Van KK", "Le Van LL", "Pham Thi MM", "Hoang Van NN",
            "Nguyen Van OO", "Tran Thi PP", "Le Van QQ", "Pham Thi RR", "Hoang Van SS",
            "Nguyen Van TT", "Tran Van UU", "Le Thi VV", "Pham Van WW", "Hoang Thi XX"
        };

        var students = new List<Student>();
        var random = new Random(42);
        for (int i = 1; i <= 50; i++)
        {
            students.Add(new Student
            {
                StudentId = i,
                FullName = vietnameseNames[i - 1],
                Email = $"student{i}@university.edu.vn",
                DateOfBirth = new DateTime(2000 + random.Next(0, 5), random.Next(1, 13), random.Next(1, 29))
            });
        }
        modelBuilder.Entity<Student>().HasData(students);

        // Enrollments (500+ enrollments)
        var enrollments = new List<Enrollment>();
        var statuses = new[] { "Active", "Completed", "Dropped" };
        var enrollmentDate = new DateTime(2024, 1, 15);

        int enrollmentId = 1;
        for (int studentId = 1; studentId <= 50; studentId++)
        {
            // Each student enrolls in 10-12 courses
            var numEnrollments = 10 + random.Next(0, 3);
            var courseIds = Enumerable.Range(1, 20).OrderBy(_ => random.Next()).Take(numEnrollments).ToList();

            foreach (var courseId in courseIds)
            {
                enrollments.Add(new Enrollment
                {
                    EnrollmentId = enrollmentId++,
                    StudentId = studentId,
                    CourseId = courseId,
                    EnrollDate = enrollmentDate.AddDays(random.Next(0, 60)),
                    Status = statuses[random.Next(statuses.Length)]
                });
            }
        }
        modelBuilder.Entity<Enrollment>().HasData(enrollments);
    }
}
