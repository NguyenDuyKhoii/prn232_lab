using Microsoft.AspNetCore.Mvc;
using PRN232.LMS.Services.Common;
using PRN232.LMS.Services.Requests;
using PRN232.LMS.Services.Responses;
using PRN232.LMS.Services.Interfaces;

namespace PRN232.LMS.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class EnrollmentsController : ControllerBase
{
    private readonly IEnrollmentService _enrollmentService;

    public EnrollmentsController(IEnrollmentService enrollmentService)
    {
        _enrollmentService = enrollmentService;
    }

    /// <summary>
    /// Get all enrollments with pagination, search, sorting, and field selection
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(PaginatedResponse<List<EnrollmentResponse>>), StatusCodes.Status200OK)]
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

        var result = await _enrollmentService.GetAllAsync(queryParams);
        return Ok(result);
    }

    /// <summary>
    /// Get an enrollment by ID
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ApiResponse<EnrollmentResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<EnrollmentResponse>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int id, [FromQuery] string? expand = null)
    {
        var result = await _enrollmentService.GetByIdAsync(id, expand);
        if (!result.Success && result.Errors?.Contains("404") == true)
        {
            return NotFound(result);
        }
        return Ok(result);
    }

    /// <summary>
    /// Create a new enrollment
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<EnrollmentResponse>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<EnrollmentResponse>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateEnrollmentRequest request)
    {
        var result = await _enrollmentService.CreateAsync(request);
        if (!result.Success)
        {
            return BadRequest(result);
        }
        return CreatedAtAction(nameof(GetById), new { id = result.Data?.EnrollmentId }, result);
    }

    /// <summary>
    /// Update an existing enrollment
    /// </summary>
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(ApiResponse<EnrollmentResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<EnrollmentResponse>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<EnrollmentResponse>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateEnrollmentRequest request)
    {
        var result = await _enrollmentService.UpdateAsync(id, request);
        if (!result.Success)
        {
            if (result.Errors?.Contains("404") == true)
                return NotFound(result);
            return BadRequest(result);
        }
        return Ok(result);
    }

    /// <summary>
    /// Delete an enrollment
    /// </summary>
    [HttpDelete("{id}")]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await _enrollmentService.DeleteAsync(id);
        if (!result.Success)
        {
            return NotFound(result);
        }
        return Ok(result);
    }
}
