using Microsoft.AspNetCore.Mvc;
using OpenSenseMapAPI.Models;
using OpenSenseMapAPI.Services;

namespace OpenSenseMapAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class BoxesController : ControllerBase
{
    private readonly IOpenSenseMapService _openSenseMapService;
    private readonly ITokenCacheService _tokenCacheService;
    private readonly ILogger<BoxesController> _logger;

    public BoxesController(
        IOpenSenseMapService openSenseMapService,
        ITokenCacheService tokenCacheService,
        ILogger<BoxesController> logger)
    {
        _openSenseMapService = openSenseMapService;
        _tokenCacheService = tokenCacheService;
        _logger = logger;
    }

    /// <summary>
    /// Create a new sense box (requires authentication)
    /// </summary>
    /// <param name="request">Sense box details</param>
    /// <returns>Created sense box information</returns>
    [HttpPost]
    [ProducesResponseType(typeof(CreateBoxResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> CreateNewSenseBox([FromBody] NewSenseBoxRequest request)
    {
        if (!ModelState.IsValid)
        {
            var errors = ModelState.Values
                .SelectMany(v => v.Errors)
                .Select(e => e.ErrorMessage)
                .ToList();
            return BadRequest(ApiResponse<object>.FailureResponse("Validation failed", errors));
        }

        var token = _tokenCacheService.GetToken(request.Email);
        if (string.IsNullOrWhiteSpace(token))
        {
            return Unauthorized(ApiResponse<object>.FailureResponse(
                "User is not authenticated. Please login first."));
        }

        var result = await _openSenseMapService.CreateNewSenseBoxAsync(request, token);
        
        _logger.LogInformation("Sense box created successfully for user {Email}", request.Email);

        return Ok(result);
    }

    /// <summary>
    /// Get sense box details by ID
    /// </summary>
    /// <param name="senseBoxId">The ID of the sense box to retrieve</param>
    /// <returns>Sense box details</returns>
    [HttpGet("{senseBoxId}")]
    [ProducesResponseType(typeof(GetSenseBoxResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetSenseBoxById([FromRoute] string senseBoxId)
    {
        if (string.IsNullOrWhiteSpace(senseBoxId))
        {
            return BadRequest(ApiResponse<object>.FailureResponse("Sense box ID is required"));
        }

        var result = await _openSenseMapService.GetSenseBoxByIdAsync(senseBoxId);
        
        _logger.LogInformation("Sense box {SenseBoxId} retrieved successfully", senseBoxId);

        return Ok(result);
    }
}
