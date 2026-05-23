using Microsoft.AspNetCore.Mvc;
using PRN232.LMS.Services.Common;
using PRN232.LMS.Services.Requests;
using PRN232.LMS.Services.Responses;
using PRN232.LMS.Services.Interfaces;

namespace PRN232.LMS.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class StudentsController : ControllerBase
{
    private readonly IStudentService _studentService;

    public StudentsController(IStudentService studentService)
    {
        _studentService = studentService;
    }

    /// <summary>
    /// Get all students with pagination, search, sorting, and field selection
    /// </summary>
    /// <param name="search">Search by name or email</param>
    /// <param name="sort">Sort field (prefix with - for descending)</param>
    /// <param name="page">Page number (default: 1)</param>
    /// <param name="size">Page size (default: 10)</param>
    /// <param name="fields">Fields to include in response</param>
    /// <param name="expand">Related data to include (e.g., enrollments)</param>
    [HttpGet]
    [ProducesResponseType(typeof(PaginatedResponse<List<StudentResponse>>), StatusCodes.Status200OK)]
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

        var result = await _studentService.GetAllAsync(queryParams);
        return Ok(result);
    }

    /// <summary>
    /// Get a student by ID
    /// </summary>
    /// <param name="id">Student ID</param>
    /// <param name="expand">Related data to include (e.g., enrollments)</param>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ApiResponse<StudentResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<StudentResponse>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int id, [FromQuery] string? expand = null)
    {
        var result = await _studentService.GetByIdAsync(id, expand);
        if (!result.Success && result.Errors?.Contains("404") == true)
        {
            return NotFound(result);
        }
        return Ok(result);
    }

    /// <summary>
    /// Create a new student
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<StudentResponse>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<StudentResponse>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateStudentRequest request)
    {
        var result = await _studentService.CreateAsync(request);
        if (!result.Success)
        {
            return BadRequest(result);
        }
        return CreatedAtAction(nameof(GetById), new { id = result.Data?.StudentId }, result);
    }

    /// <summary>
    /// Update an existing student
    /// </summary>
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(ApiResponse<StudentResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<StudentResponse>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<StudentResponse>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateStudentRequest request)
    {
        var result = await _studentService.UpdateAsync(id, request);
        if (!result.Success)
        {
            if (result.Errors?.Contains("404") == true)
                return NotFound(result);
            return BadRequest(result);
        }
        return Ok(result);
    }

    /// <summary>
    /// Delete a student
    /// </summary>
    [HttpDelete("{id}")]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await _studentService.DeleteAsync(id);
        if (!result.Success)
        {
            return NotFound(result);
        }
        return Ok(result);
    }
}
