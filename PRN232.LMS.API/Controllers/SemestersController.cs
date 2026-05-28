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
public class SemestersController : ControllerBase
{
    private readonly ISemesterService _semesterService;
    private readonly IMapper _mapper;

    public SemestersController(ISemesterService semesterService, IMapper mapper)
    {
        _semesterService = semesterService;
        _mapper = mapper;
    }

    /// <summary>
    /// Get all semesters with pagination, search, sorting, and field selection
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

        var result = await _semesterService.GetAllAsync(queryParams);
        var responseData = _mapper.Map<List<SemesterResponse>>(result.Data!.Data);
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
    /// Get a semester by ID
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ApiResponse<SemesterResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<SemesterResponse>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int id, [FromQuery] string? expand = null)
    {
        var result = await _semesterService.GetByIdAsync(id, expand);
        if (!result.Success)
        {
            return NotFound(ApiResponse<SemesterResponse>.NotFound(result.Message));
        }

        var responseData = _mapper.Map<SemesterResponse>(result.Data);
        return Ok(ApiResponse<SemesterResponse>.Ok(responseData));
    }

    /// <summary>
    /// Create a new semester
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<SemesterResponse>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<SemesterResponse>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateSemesterRequest request)
    {
        var semesterBM = _mapper.Map<SemesterBM>(request);
        var result = await _semesterService.CreateAsync(semesterBM);
        if (!result.Success)
        {
            return BadRequest(ApiResponse<SemesterResponse>.BadRequest(result.Message));
        }

        var responseData = _mapper.Map<SemesterResponse>(result.Data);
        return CreatedAtAction(nameof(GetById), new { id = responseData.SemesterId }, ApiResponse<SemesterResponse>.Ok(responseData, result.Message));
    }

    /// <summary>
    /// Update an existing semester
    /// </summary>
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(ApiResponse<SemesterResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<SemesterResponse>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<SemesterResponse>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateSemesterRequest request)
    {
        var semesterBM = _mapper.Map<SemesterBM>(request);
        var result = await _semesterService.UpdateAsync(id, semesterBM);
        if (!result.Success)
        {
            if (result.StatusCode == 404)
                return NotFound(ApiResponse<SemesterResponse>.NotFound(result.Message));
            return BadRequest(ApiResponse<SemesterResponse>.BadRequest(result.Message));
        }

        var responseData = _mapper.Map<SemesterResponse>(result.Data);
        return Ok(ApiResponse<SemesterResponse>.Ok(responseData, result.Message));
    }

    /// <summary>
    /// Delete a semester
    /// </summary>
    [HttpDelete("{id}")]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await _semesterService.DeleteAsync(id);
        if (!result.Success)
        {
            return NotFound(ApiResponse<bool>.NotFound(result.Message));
        }
        return Ok(ApiResponse<bool>.Ok(true, result.Message));
    }
}
