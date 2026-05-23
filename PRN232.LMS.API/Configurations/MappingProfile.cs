using AutoMapper;
using PRN232.LMS.Repositories.Entities;
using PRN232.LMS.Services.Requests;
using PRN232.LMS.Services.Responses;

namespace PRN232.LMS.API.Configurations;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        // Semester
        CreateMap<Semester, SemesterResponse>();
        CreateMap<CreateSemesterRequest, Semester>()
            .ForMember(dest => dest.SemesterId, opt => opt.Ignore());
        CreateMap<UpdateSemesterRequest, Semester>()
            .ForMember(dest => dest.SemesterId, opt => opt.Ignore())
            .ForMember(dest => dest.Courses, opt => opt.Ignore());

        // Course
        CreateMap<Course, CourseResponse>()
            .ForMember(dest => dest.SemesterName, opt => opt.MapFrom(src => src.Semester != null ? src.Semester.SemesterName : null));
        CreateMap<CreateCourseRequest, Course>()
            .ForMember(dest => dest.CourseId, opt => opt.Ignore())
            .ForMember(dest => dest.Semester, opt => opt.Ignore());
        CreateMap<UpdateCourseRequest, Course>()
            .ForMember(dest => dest.CourseId, opt => opt.Ignore())
            .ForMember(dest => dest.Semester, opt => opt.Ignore())
            .ForMember(dest => dest.Enrollments, opt => opt.Ignore());

        // Student
        CreateMap<Student, StudentResponse>();
        CreateMap<CreateStudentRequest, Student>()
            .ForMember(dest => dest.StudentId, opt => opt.Ignore())
            .ForMember(dest => dest.Enrollments, opt => opt.Ignore());
        CreateMap<UpdateStudentRequest, Student>()
            .ForMember(dest => dest.StudentId, opt => opt.Ignore())
            .ForMember(dest => dest.Enrollments, opt => opt.Ignore());

        // Enrollment
        CreateMap<Enrollment, EnrollmentResponse>()
            .ForMember(dest => dest.StudentName, opt => opt.MapFrom(src => src.Student != null ? src.Student.FullName : null))
            .ForMember(dest => dest.CourseName, opt => opt.MapFrom(src => src.Course != null ? src.Course.CourseName : null));
        CreateMap<CreateEnrollmentRequest, Enrollment>()
            .ForMember(dest => dest.EnrollmentId, opt => opt.Ignore())
            .ForMember(dest => dest.Student, opt => opt.Ignore())
            .ForMember(dest => dest.Course, opt => opt.Ignore());
        CreateMap<UpdateEnrollmentRequest, Enrollment>()
            .ForMember(dest => dest.EnrollmentId, opt => opt.Ignore())
            .ForMember(dest => dest.Student, opt => opt.Ignore())
            .ForMember(dest => dest.Course, opt => opt.Ignore());

        // Subject
        CreateMap<Subject, SubjectResponse>();
        CreateMap<CreateSubjectRequest, Subject>()
            .ForMember(dest => dest.SubjectId, opt => opt.Ignore());
        CreateMap<UpdateSubjectRequest, Subject>()
            .ForMember(dest => dest.SubjectId, opt => opt.Ignore());
    }
}
