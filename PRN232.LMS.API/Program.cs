using Microsoft.EntityFrameworkCore;
using PRN232.LMS.API.Configurations;
using PRN232.LMS.Repositories.DbContexts;
using PRN232.LMS.Repositories.Implementations;
using PRN232.LMS.Repositories.Interfaces;
using PRN232.LMS.Services.Implementations;
using PRN232.LMS.Services.Interfaces;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container

// Database
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<LmsDbContext>(options =>
    options.UseSqlServer(connectionString));

// AutoMapper
builder.Services.AddAutoMapper(typeof(MappingProfile));

// Repositories
builder.Services.AddScoped<ISemesterRepository, SemesterRepository>();
builder.Services.AddScoped<ICourseRepository, CourseRepository>();
builder.Services.AddScoped<IStudentRepository, StudentRepository>();
builder.Services.AddScoped<IEnrollmentRepository, EnrollmentRepository>();
builder.Services.AddScoped<ISubjectRepository, SubjectRepository>();

// Services
builder.Services.AddScoped<ISemesterService, SemesterService>();
builder.Services.AddScoped<ICourseService, CourseService>();
builder.Services.AddScoped<IStudentService, StudentService>();
builder.Services.AddScoped<IEnrollmentService, EnrollmentService>();
builder.Services.AddScoped<ISubjectService, SubjectService>();

// Controllers
builder.Services.AddControllers();

// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "LMS API",
        Version = "v1",
        Description = "Learning Management System RESTful API",
        Contact = new OpenApiContact
        {
            Name = "PRN232 Team",
            Email = "prn232@university.edu.vn"
        }
    });
    c.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, "PRN232.LMS.API.xml"), true);
    c.EnableAnnotations();
});

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "LMS API v1");
        c.RoutePrefix = "swagger";
    });
}

if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

app.UseAuthorization();

app.MapControllers();

// Ensure database is created and seeded
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<LmsDbContext>();
    context.Database.EnsureCreated();
}

app.Run();

public partial class Program { }