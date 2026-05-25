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
public class SubjectsController : ControllerBase
{
    private readonly ISubjectService _subjectService;
    private readonly IMapper _mapper;

    public SubjectsController(ISubjectService subjectService, IMapper mapper)
    {
        _subjectService = subjectService;
        _mapper = mapper;
    }

    /// <summary>
    /// Get all subjects with pagination, search, sorting, and field selection
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

        var result = await _subjectService.GetAllAsync(queryParams);
        var responseData = _mapper.Map<List<SubjectResponse>>(result.Data!.Data);
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
    /// Get a subject by ID
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ApiResponse<SubjectResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<SubjectResponse>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await _subjectService.GetByIdAsync(id);
        if (!result.Success)
        {
            return NotFound(ApiResponse<SubjectResponse>.NotFound(result.Message));
        }

        var responseData = _mapper.Map<SubjectResponse>(result.Data);
        return Ok(ApiResponse<SubjectResponse>.Ok(responseData));
    }

    /// <summary>
    /// Create a new subject
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<SubjectResponse>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<SubjectResponse>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateSubjectRequest request)
    {
        var subjectBM = _mapper.Map<SubjectBM>(request);
        var result = await _subjectService.CreateAsync(subjectBM);
        if (!result.Success)
        {
            return BadRequest(ApiResponse<SubjectResponse>.BadRequest(result.Message));
        }

        var responseData = _mapper.Map<SubjectResponse>(result.Data);
        return CreatedAtAction(nameof(GetById), new { id = responseData.SubjectId }, ApiResponse<SubjectResponse>.Ok(responseData, result.Message));
    }

    /// <summary>
    /// Update an existing subject
    /// </summary>
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(ApiResponse<SubjectResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<SubjectResponse>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<SubjectResponse>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateSubjectRequest request)
    {
        var subjectBM = _mapper.Map<SubjectBM>(request);
        var result = await _subjectService.UpdateAsync(id, subjectBM);
        if (!result.Success)
        {
            if (result.StatusCode == 404)
                return NotFound(ApiResponse<SubjectResponse>.NotFound(result.Message));
            return BadRequest(ApiResponse<SubjectResponse>.BadRequest(result.Message));
        }

        var responseData = _mapper.Map<SubjectResponse>(result.Data);
        return Ok(ApiResponse<SubjectResponse>.Ok(responseData, result.Message));
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
            return NotFound(ApiResponse<bool>.NotFound(result.Message));
        }
        return Ok(ApiResponse<bool>.Ok(true, result.Message));
    }
}
