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
public class StudentsController : ControllerBase
{
    private readonly IStudentService _studentService;
    private readonly IMapper _mapper;

    public StudentsController(IStudentService studentService, IMapper mapper)
    {
        _studentService = studentService;
        _mapper = mapper;
    }

    /// <summary>
    /// Get all students with pagination, search, sorting, and field selection
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

        var result = await _studentService.GetAllAsync(queryParams);
        var responseData = _mapper.Map<List<StudentResponse>>(result.Data!.Data);
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
    /// Get a student by ID
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ApiResponse<StudentResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<StudentResponse>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int id, [FromQuery] string? expand = null)
    {
        var result = await _studentService.GetByIdAsync(id, expand);
        if (!result.Success)
        {
            return NotFound(ApiResponse<StudentResponse>.NotFound(result.Message));
        }

        var responseData = _mapper.Map<StudentResponse>(result.Data);
        return Ok(ApiResponse<StudentResponse>.Ok(responseData));
    }

    /// <summary>
    /// Create a new student
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<StudentResponse>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<StudentResponse>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateStudentRequest request)
    {
        var studentBM = _mapper.Map<StudentBM>(request);
        var result = await _studentService.CreateAsync(studentBM);
        if (!result.Success)
        {
            return BadRequest(ApiResponse<StudentResponse>.BadRequest(result.Message));
        }

        var responseData = _mapper.Map<StudentResponse>(result.Data);
        return CreatedAtAction(nameof(GetById), new { id = responseData.StudentId }, ApiResponse<StudentResponse>.Ok(responseData, result.Message));
    }

    /// <summary>
    /// Update an existing student
    /// </summary>
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(ApiResponse<StudentResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<StudentResponse>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<StudentResponse>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateStudentRequest request)
    {
        var studentBM = _mapper.Map<StudentBM>(request);
        var result = await _studentService.UpdateAsync(id, studentBM);
        if (!result.Success)
        {
            if (result.StatusCode == 404)
                return NotFound(ApiResponse<StudentResponse>.NotFound(result.Message));
            return BadRequest(ApiResponse<StudentResponse>.BadRequest(result.Message));
        }

        var responseData = _mapper.Map<StudentResponse>(result.Data);
        return Ok(ApiResponse<StudentResponse>.Ok(responseData, result.Message));
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
            return NotFound(ApiResponse<bool>.NotFound(result.Message));
        }
        return Ok(ApiResponse<bool>.Ok(true, result.Message));
    }

    /// <summary>
    /// Test endpoint to trigger a 500 Internal Server Error for global exception handling validation
    /// </summary>
    [HttpGet("test-500")]
    public IActionResult Trigger500Error()
    {
        throw new Exception("Test global exception handling middleware - Simulated 500 Server Error");
    }
}
