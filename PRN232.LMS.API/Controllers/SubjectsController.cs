using Microsoft.AspNetCore.Mvc;
using PRN232.LMS.Services.Common;
using PRN232.LMS.Services.Requests;
using PRN232.LMS.Services.Responses;
using PRN232.LMS.Services.Interfaces;

namespace PRN232.LMS.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class SubjectsController : ControllerBase
{
    private readonly ISubjectService _subjectService;

    public SubjectsController(ISubjectService subjectService)
    {
        _subjectService = subjectService;
    }

    /// <summary>
    /// Get all subjects with pagination, search, sorting, and field selection
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(PaginatedResponse<List<SubjectResponse>>), StatusCodes.Status200OK)]
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

        var result = await _subjectService.GetAllAsync(queryParams);
        return Ok(result);
    }

    /// <summary>
    /// Get a subject by ID
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ApiResponse<SubjectResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<SubjectResponse>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await _subjectService.GetByIdAsync(id);
        if (!result.Success && result.Errors?.Contains("404") == true)
        {
            return NotFound(result);
        }
        return Ok(result);
    }

    /// <summary>
    /// Create a new subject
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<SubjectResponse>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<SubjectResponse>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateSubjectRequest request)
    {
        var result = await _subjectService.CreateAsync(request);
        if (!result.Success)
        {
            return BadRequest(result);
        }
        return CreatedAtAction(nameof(GetById), new { id = result.Data?.SubjectId }, result);
    }

    /// <summary>
    /// Update an existing subject
    /// </summary>
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(ApiResponse<SubjectResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<SubjectResponse>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<SubjectResponse>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateSubjectRequest request)
    {
        var result = await _subjectService.UpdateAsync(id, request);
        if (!result.Success)
        {
            if (result.Errors?.Contains("404") == true)
                return NotFound(result);
            return BadRequest(result);
        }
        return Ok(result);
    }

    /// <summary>
    /// Delete a subject
    /// </summary>
    [HttpDelete("{id}")]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await _subjectService.DeleteAsync(id);
        if (!result.Success)
        {
            return NotFound(result);
        }
        return Ok(result);
    }
}
