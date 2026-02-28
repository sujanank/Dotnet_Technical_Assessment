using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;
using Moq;
using OpenSenseMapAPI.Controllers;
using OpenSenseMapAPI.Models;
using OpenSenseMapAPI.Services;
using Xunit;

namespace OpenSenseMapAPI.Tests.Controllers;

public class UsersControllerTests
{
    private readonly Mock<IOpenSenseMapService> _openSenseMapServiceMock;
    private readonly Mock<ITokenCacheService> _tokenCacheServiceMock;
    private readonly Mock<ILogger<UsersController>> _loggerMock;
    private readonly UsersController _controller;

    public UsersControllerTests()
    {
        _openSenseMapServiceMock = new Mock<IOpenSenseMapService>();
        _tokenCacheServiceMock = new Mock<ITokenCacheService>();
        _loggerMock = new Mock<ILogger<UsersController>>();
        _controller = new UsersController(
            _openSenseMapServiceMock.Object,
            _tokenCacheServiceMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task Register_ValidRequest_ReturnsOkResult()
    {
        var request = new RegisterRequest
        {
            Name = "Test User",
            Email = "test@example.com",
            Password = "password123"
        };

        var expectedResponse = new RegisterResponse
        {
            Code = "created",
            Message = "Successfully registered new user.",
            Token = "test-token-123",
            Data = new object()
        };
        _openSenseMapServiceMock.Setup(s => s.RegisterUserAsync(request))
            .ReturnsAsync(expectedResponse);

        var result = await _controller.Register(request);

        var okResult = Assert.IsType<OkObjectResult>(result);
        var response = Assert.IsType<RegisterResponse>(okResult.Value);
        Assert.Equal("created", response.Code);
        Assert.Equal("Successfully registered new user.", response.Message);
        Assert.Equal("test-token-123", response.Token);
    }

    [Fact]
    public async Task Register_InvalidModelState_ReturnsBadRequest()
    {
        _controller.ModelState.AddModelError("Email", "Email is required");

        var request = new RegisterRequest
        {
            Name = "Test User",
            Password = "password123"
        };

        var result = await _controller.Register(request);

        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        var response = Assert.IsType<ApiResponse<object>>(badRequestResult.Value);
        Assert.False(response.Success);
        Assert.Equal("Validation failed", response.Message);
    }

    [Fact]
    public async Task Login_ValidCredentials_ReturnsOkAndCachesToken()
    {
        var request = new LoginRequest
        {
            Email = "test@example.com",
            Password = "password123"
        };

        var loginResponse = new LoginResponse
        {
            Code = "Authorized",
            Message = "Successfully signed in",
            Token = "test-token",
            RefreshToken = "refresh-token",
            Data = new object()
        };

        _openSenseMapServiceMock.Setup(s => s.LoginAsync(request))
            .ReturnsAsync(loginResponse);

        var result = await _controller.Login(request);

        var okResult = Assert.IsType<OkObjectResult>(result);
        var response = Assert.IsType<LoginResponse>(okResult.Value);
        Assert.Equal("Authorized", response.Code);
        Assert.Equal("Successfully signed in", response.Message);
        Assert.Equal("test-token", response.Token);
        Assert.Equal("refresh-token", response.RefreshToken);

        _tokenCacheServiceMock.Verify(t => t.SetToken(request.Email, loginResponse.Token), Times.Once);
    }

    [Fact]
    public async Task Login_InvalidModelState_ReturnsBadRequest()
    {
        _controller.ModelState.AddModelError("Password", "Password is required");

        var request = new LoginRequest
        {
            Email = "test@example.com"
        };

        var result = await _controller.Login(request);

        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        var response = Assert.IsType<ApiResponse<object>>(badRequestResult.Value);
        Assert.False(response.Success);
    }

    [Fact]
    public async Task Logout_WithValidToken_ReturnsOkAndClearsToken()
    {
        var email = "test@example.com";
        var token = "test-token";

        // Setup HttpContext with Authorization header
        var httpContext = new DefaultHttpContext();
        httpContext.Request.Headers["Authorization"] = $"Bearer {token}";
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = httpContext
        };

        _tokenCacheServiceMock.Setup(t => t.GetEmailByToken(token))
            .Returns(email);

        _openSenseMapServiceMock.Setup(s => s.LogoutAsync(token))
            .ReturnsAsync(new { message = "Logged out successfully" });

        var result = await _controller.Logout();

        var okResult = Assert.IsType<OkObjectResult>(result);
        var response = Assert.IsType<ApiResponse<object>>(okResult.Value);
        Assert.True(response.Success);
        Assert.Equal("Logout successful", response.Message);

        _tokenCacheServiceMock.Verify(t => t.RemoveToken(email), Times.Once);
    }

    [Fact]
    public async Task Logout_WithoutAuthorizationHeader_ReturnsUnauthorized()
    {
        var httpContext = new DefaultHttpContext();
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = httpContext
        };

        var result = await _controller.Logout();

        var unauthorizedResult = Assert.IsType<UnauthorizedObjectResult>(result);
        var response = Assert.IsType<ApiResponse<object>>(unauthorizedResult.Value);
        Assert.False(response.Success);
        Assert.Contains("Authorization header", response.Message);
    }

    [Fact]
    public async Task Logout_WithInvalidToken_ReturnsUnauthorized()
    {
        var token = "invalid-token";

        var httpContext = new DefaultHttpContext();
        httpContext.Request.Headers["Authorization"] = $"Bearer {token}";
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = httpContext
        };

        _tokenCacheServiceMock.Setup(t => t.GetEmailByToken(token))
            .Returns((string?)null);

        var result = await _controller.Logout();

        var unauthorizedResult = Assert.IsType<UnauthorizedObjectResult>(result);
        var response = Assert.IsType<ApiResponse<object>>(unauthorizedResult.Value);
        Assert.False(response.Success);
        Assert.Contains("not authenticated", response.Message);
    }

   
}
