using Microsoft.AspNetCore.Mvc;
using OpenSenseMapAPI.Models;
using OpenSenseMapAPI.Services;

namespace OpenSenseMapAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class UsersController : ControllerBase
{
    private readonly IOpenSenseMapService _openSenseMapService;
    private readonly ITokenCacheService _tokenCacheService;
    private readonly ILogger<UsersController> _logger;

    public UsersController(
        IOpenSenseMapService openSenseMapService,
        ITokenCacheService tokenCacheService,
        ILogger<UsersController> logger)
    {
        _openSenseMapService = openSenseMapService;
        _tokenCacheService = tokenCacheService;
        _logger = logger;
    }

    /// <summary>
    /// Register a new user in OpenSenseMap
    /// </summary>
    /// <param name="request">User registration details</param>
    /// <returns>Registration response</returns>
    [HttpPost("register")]
    [ProducesResponseType(typeof(RegisterResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        if (!ModelState.IsValid)
        {
            var errors = ModelState.Values
                .SelectMany(v => v.Errors)
                .Select(e => e.ErrorMessage)
                .ToList();
            return BadRequest(ApiResponse<object>.FailureResponse("Validation failed", errors));
        }

        var result = await _openSenseMapService.RegisterUserAsync(request);
        return Ok(result);
    }

    /// <summary>
    /// Login user and store authentication token
    /// </summary>
    /// <param name="request">User login credentials</param>
    /// <returns>Login response with token</returns>
    [HttpPost("login")]
    [ProducesResponseType(typeof(LoginResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        if (!ModelState.IsValid)
        {
            var errors = ModelState.Values
                .SelectMany(v => v.Errors)
                .Select(e => e.ErrorMessage)
                .ToList();
            return BadRequest(ApiResponse<object>.FailureResponse("Validation failed", errors));
        }

        var result = await _openSenseMapService.LoginAsync(request);
        
        _tokenCacheService.SetToken(request.Email, result.Token);
        
        _logger.LogInformation("User {Email} logged in successfully and token cached", request.Email);

        return Ok(result);
    }

    /// <summary>
    /// Logout user and clear cached token
    /// </summary>
    /// <returns>Logout response</returns>
    [HttpPost("logout")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Logout()
    {
        // Extract token from Authorization header
        var authHeader = Request.Headers["Authorization"].ToString();
        
        if (string.IsNullOrWhiteSpace(authHeader) || !authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
        {
            return Unauthorized(ApiResponse<object>.FailureResponse("Authorization header with Bearer token is required"));
        }

        var token = authHeader.Substring("Bearer ".Length).Trim();
        
        if (string.IsNullOrWhiteSpace(token))
        {
            return Unauthorized(ApiResponse<object>.FailureResponse("Invalid token"));
        }

        // Find email associated with this token
        var email = _tokenCacheService.GetEmailByToken(token);
        
        if (string.IsNullOrWhiteSpace(email))
        {
            return Unauthorized(ApiResponse<object>.FailureResponse("User is not authenticated or token has expired"));
        }

        var result = await _openSenseMapService.LogoutAsync(token);
        
        _tokenCacheService.RemoveToken(email);
        
        _logger.LogInformation("User {Email} logged out successfully and token cleared", email);
        
        return Ok(ApiResponse<object>.SuccessResponse(result, "Logout successful"));
    }
}
