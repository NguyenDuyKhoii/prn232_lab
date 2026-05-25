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
public class EnrollmentsController : ControllerBase
{
    private readonly IEnrollmentService _enrollmentService;
    private readonly IMapper _mapper;

    public EnrollmentsController(IEnrollmentService enrollmentService, IMapper mapper)
    {
        _enrollmentService = enrollmentService;
        _mapper = mapper;
    }

    /// <summary>
    /// Get all enrollments with pagination, search, sorting, and field selection
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

        var result = await _enrollmentService.GetAllAsync(queryParams);
        var responseData = _mapper.Map<List<EnrollmentResponse>>(result.Data!.Data);
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
    /// Get an enrollment by ID
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ApiResponse<EnrollmentResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<EnrollmentResponse>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int id, [FromQuery] string? expand = null)
    {
        var result = await _enrollmentService.GetByIdAsync(id, expand);
        if (!result.Success)
        {
            return NotFound(ApiResponse<EnrollmentResponse>.NotFound(result.Message));
        }

        var responseData = _mapper.Map<EnrollmentResponse>(result.Data);
        return Ok(ApiResponse<EnrollmentResponse>.Ok(responseData));
    }

    /// <summary>
    /// Create a new enrollment
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<EnrollmentResponse>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<EnrollmentResponse>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateEnrollmentRequest request)
    {
        var enrollmentBM = _mapper.Map<EnrollmentBM>(request);
        var result = await _enrollmentService.CreateAsync(enrollmentBM);
        if (!result.Success)
        {
            return BadRequest(ApiResponse<EnrollmentResponse>.BadRequest(result.Message));
        }

        var responseData = _mapper.Map<EnrollmentResponse>(result.Data);
        return CreatedAtAction(nameof(GetById), new { id = responseData.EnrollmentId }, ApiResponse<EnrollmentResponse>.Ok(responseData, result.Message));
    }

    /// <summary>
    /// Update an existing enrollment
    /// </summary>
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(ApiResponse<EnrollmentResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<EnrollmentResponse>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<EnrollmentResponse>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateEnrollmentRequest request)
    {
        var enrollmentBM = _mapper.Map<EnrollmentBM>(request);
        var result = await _enrollmentService.UpdateAsync(id, enrollmentBM);
        if (!result.Success)
        {
            if (result.StatusCode == 404)
                return NotFound(ApiResponse<EnrollmentResponse>.NotFound(result.Message));
            return BadRequest(ApiResponse<EnrollmentResponse>.BadRequest(result.Message));
        }

        var responseData = _mapper.Map<EnrollmentResponse>(result.Data);
        return Ok(ApiResponse<EnrollmentResponse>.Ok(responseData, result.Message));
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
            return NotFound(ApiResponse<bool>.NotFound(result.Message));
        }
        return Ok(ApiResponse<bool>.Ok(true, result.Message));
    }
}
