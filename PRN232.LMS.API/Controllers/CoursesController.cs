using Microsoft.AspNetCore.Mvc;
using AutoMapper;
using PRN232.LMS.Services.Common;
using PRN232.LMS.Services.BusinessModels;
using PRN232.LMS.API.Requests;
using PRN232.LMS.API.Responses;
using PRN232.LMS.Services.Interfaces;

using PRN232.LMS.API.Helpers;

namespace PRN232.LMS.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class CoursesController : ControllerBase
{
    private readonly ICourseService _courseService;
    private readonly IEnrollmentService _enrollmentService;
    private readonly IMapper _mapper;

    public CoursesController(ICourseService courseService, IEnrollmentService enrollmentService, IMapper mapper)
    {
        _courseService = courseService;
        _enrollmentService = enrollmentService;
        _mapper = mapper;
    }

    /// <summary>
    /// Get all courses with pagination, search, sorting, and field selection
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(PaginatedResponse<List<object>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(
        [FromQuery] string? search = null,
        [FromQuery] string? sort = null,
        [FromQuery] int page = 1,
        [FromQuery] int size = 10,
        [FromQuery] string? fields = null,
        [FromQuery] string? expand = null)
    {
        var queryParams = new QueryParams
        {
            Search = search,
            Sort = sort,
            Page = page,
            Size = size,
            Fields = fields,
            Expand = expand
        };

        var result = await _courseService.GetAllAsync(queryParams);
        var responseData = _mapper.Map<List<CourseResponse>>(result.Data!.Data);
        var shapedData = FieldHelper.ShapeDataList(responseData, fields);

        var pagination = new PaginationMetadata
        {
            Page = result.Data.Page,
            PageSize = result.Data.PageSize,
            TotalItems = result.Data.TotalItems,
            TotalPages = result.Data.TotalPages
        };

        return Ok(PaginatedResponse<List<object?>>.Ok(shapedData, pagination, result.Message));
    }

    /// <summary>
    /// Get a course by ID
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ApiResponse<CourseResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<CourseResponse>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int id, [FromQuery] string? expand = null)
    {
        var result = await _courseService.GetByIdAsync(id, expand);
        if (!result.Success)
        {
            return NotFound(ApiResponse<CourseResponse>.NotFound(result.Message));
        }

        var responseData = _mapper.Map<CourseResponse>(result.Data);
        return Ok(ApiResponse<CourseResponse>.Ok(responseData));
    }

    /// <summary>
    /// Create a new course
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<CourseResponse>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<CourseResponse>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateCourseRequest request)
    {
        var courseBM = _mapper.Map<CourseBM>(request);
        var result = await _courseService.CreateAsync(courseBM);
        if (!result.Success)
        {
            return BadRequest(ApiResponse<CourseResponse>.BadRequest(result.Message));
        }

        var responseData = _mapper.Map<CourseResponse>(result.Data);
        return CreatedAtAction(nameof(GetById), new { id = responseData.CourseId }, ApiResponse<CourseResponse>.Ok(responseData, result.Message));
    }

    /// <summary>
    /// Update an existing course
    /// </summary>
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(ApiResponse<CourseResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<CourseResponse>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<CourseResponse>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateCourseRequest request)
    {
        var courseBM = _mapper.Map<CourseBM>(request);
        var result = await _courseService.UpdateAsync(id, courseBM);
        if (!result.Success)
        {
            if (result.StatusCode == 404)
                return NotFound(ApiResponse<CourseResponse>.NotFound(result.Message));
            return BadRequest(ApiResponse<CourseResponse>.BadRequest(result.Message));
        }

        var responseData = _mapper.Map<CourseResponse>(result.Data);
        return Ok(ApiResponse<CourseResponse>.Ok(responseData, result.Message));
    }

    /// <summary>
    /// Delete a course
    /// </summary>
    [HttpDelete("{id}")]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await _courseService.DeleteAsync(id);
        if (!result.Success)
        {
            return NotFound(ApiResponse<bool>.NotFound(result.Message));
        }
        return Ok(ApiResponse<bool>.Ok(true, result.Message));
    }

    /// <summary>
    /// Get all enrollments for a specific course (Nested sub-resource under courses)
    /// </summary>
    [HttpGet("{courseId}/enrollments")]
    [ProducesResponseType(typeof(PaginatedResponse<List<EnrollmentResponse>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetEnrollments(
        int courseId,
        [FromQuery] int page = 1,
        [FromQuery] int size = 10)
    {
        var result = await _enrollmentService.GetByCourseIdAsync(courseId, page, size);
        var responseData = _mapper.Map<List<EnrollmentResponse>>(result.Data!.Data);

        var pagination = new PaginationMetadata
        {
            Page = result.Data.Page,
            PageSize = result.Data.PageSize,
            TotalItems = result.Data.TotalItems,
            TotalPages = result.Data.TotalPages
        };

        return Ok(PaginatedResponse<List<EnrollmentResponse>>.Ok(responseData, pagination, "Course enrollments retrieved successfully"));
    }
}
