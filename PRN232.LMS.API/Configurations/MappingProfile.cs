using AutoMapper;
using PRN232.LMS.Repositories.Entities;
using PRN232.LMS.Services.BusinessModels;
using PRN232.LMS.API.Requests;
using PRN232.LMS.API.Responses;

namespace PRN232.LMS.API.Configurations;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        // ========== Semester ==========
        // Entity <-> BusinessModel
        CreateMap<Semester, SemesterBM>();
        CreateMap<SemesterBM, Semester>()
            .ForMember(dest => dest.Courses, opt => opt.Ignore());

        // BusinessModel -> Response
        CreateMap<SemesterBM, SemesterResponse>();

        // Request -> BusinessModel
        CreateMap<CreateSemesterRequest, SemesterBM>()
            .ForMember(dest => dest.SemesterId, opt => opt.Ignore())
            .ForMember(dest => dest.Courses, opt => opt.Ignore());
        CreateMap<UpdateSemesterRequest, SemesterBM>()
            .ForMember(dest => dest.SemesterId, opt => opt.Ignore())
            .ForMember(dest => dest.Courses, opt => opt.Ignore());

        // ========== Course ==========
        // Entity -> BusinessModel
        CreateMap<Course, CourseBM>()
            .ForMember(dest => dest.SemesterName, opt => opt.MapFrom(src => src.Semester != null ? src.Semester.SemesterName : null));
        CreateMap<CourseBM, Course>()
            .ForMember(dest => dest.Semester, opt => opt.Ignore())
            .ForMember(dest => dest.Enrollments, opt => opt.Ignore());

        // BusinessModel -> Response
        CreateMap<CourseBM, CourseResponse>();

        // Request -> BusinessModel
        CreateMap<CreateCourseRequest, CourseBM>()
            .ForMember(dest => dest.CourseId, opt => opt.Ignore())
            .ForMember(dest => dest.SemesterName, opt => opt.Ignore())
            .ForMember(dest => dest.Enrollments, opt => opt.Ignore());
        CreateMap<UpdateCourseRequest, CourseBM>()
            .ForMember(dest => dest.CourseId, opt => opt.Ignore())
            .ForMember(dest => dest.SemesterName, opt => opt.Ignore())
            .ForMember(dest => dest.Enrollments, opt => opt.Ignore());

        // ========== Student ==========
        // Entity <-> BusinessModel
        CreateMap<Student, StudentBM>();
        CreateMap<StudentBM, Student>()
            .ForMember(dest => dest.Enrollments, opt => opt.Ignore());

        // BusinessModel -> Response
        CreateMap<StudentBM, StudentResponse>();

        // Request -> BusinessModel
        CreateMap<CreateStudentRequest, StudentBM>()
            .ForMember(dest => dest.StudentId, opt => opt.Ignore())
            .ForMember(dest => dest.Enrollments, opt => opt.Ignore());
        CreateMap<UpdateStudentRequest, StudentBM>()
            .ForMember(dest => dest.StudentId, opt => opt.Ignore())
            .ForMember(dest => dest.Enrollments, opt => opt.Ignore());

        // ========== Enrollment ==========
        // Entity -> BusinessModel
        CreateMap<Enrollment, EnrollmentBM>()
            .ForMember(dest => dest.StudentName, opt => opt.MapFrom(src => src.Student != null ? src.Student.FullName : null))
            .ForMember(dest => dest.CourseName, opt => opt.MapFrom(src => src.Course != null ? src.Course.CourseName : null));
        CreateMap<EnrollmentBM, Enrollment>()
            .ForMember(dest => dest.Student, opt => opt.Ignore())
            .ForMember(dest => dest.Course, opt => opt.Ignore());

        // BusinessModel -> Response
        CreateMap<EnrollmentBM, EnrollmentResponse>();

        // Request -> BusinessModel
        CreateMap<CreateEnrollmentRequest, EnrollmentBM>()
            .ForMember(dest => dest.EnrollmentId, opt => opt.Ignore())
            .ForMember(dest => dest.StudentName, opt => opt.Ignore())
            .ForMember(dest => dest.CourseName, opt => opt.Ignore());
        CreateMap<UpdateEnrollmentRequest, EnrollmentBM>()
            .ForMember(dest => dest.EnrollmentId, opt => opt.Ignore())
            .ForMember(dest => dest.StudentName, opt => opt.Ignore())
            .ForMember(dest => dest.CourseName, opt => opt.Ignore());

        // ========== Subject ==========
        // Entity <-> BusinessModel
        CreateMap<Subject, SubjectBM>();
        CreateMap<SubjectBM, Subject>();

        // BusinessModel -> Response
        CreateMap<SubjectBM, SubjectResponse>();

        // Request -> BusinessModel
        CreateMap<CreateSubjectRequest, SubjectBM>()
            .ForMember(dest => dest.SubjectId, opt => opt.Ignore());
        CreateMap<UpdateSubjectRequest, SubjectBM>()
            .ForMember(dest => dest.SubjectId, opt => opt.Ignore());
    }
}
