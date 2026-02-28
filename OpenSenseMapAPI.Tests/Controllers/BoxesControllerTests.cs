using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using OpenSenseMapAPI.Controllers;
using OpenSenseMapAPI.Models;
using OpenSenseMapAPI.Services;
using Xunit;

namespace OpenSenseMapAPI.Tests.Controllers;

public class BoxesControllerTests
{
    private readonly Mock<IOpenSenseMapService> _openSenseMapServiceMock;
    private readonly Mock<ITokenCacheService> _tokenCacheServiceMock;
    private readonly Mock<ILogger<BoxesController>> _loggerMock;
    private readonly BoxesController _controller;

    public BoxesControllerTests()
    {
        _openSenseMapServiceMock = new Mock<IOpenSenseMapService>();
        _tokenCacheServiceMock = new Mock<ITokenCacheService>();
        _loggerMock = new Mock<ILogger<BoxesController>>();
        _controller = new BoxesController(
            _openSenseMapServiceMock.Object,
            _tokenCacheServiceMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task CreateNewSenseBox_WithValidTokenAndRequest_ReturnsOk()
    {
        var request = new NewSenseBoxRequest
        {
            Email = "test@example.com",
            Name = "Test Box",
            Exposure = "indoor",
            Model = "homeV2Wifi",
            Location = new LocationDto
            {
                Lat = 90,
                Lng = 180,
                Height = 100
            }
        };

        var token = "valid-token";
        var expectedResponse = new CreateBoxResponse
        {
            Id = "test-box-id",
            Name = "Test Box",
            Exposure = "indoor",
            Model = "homeV2Wifi",
            CreatedAt = "2026-02-28T06:22:04.143Z",
            UpdatedAt = "2026-02-28T06:22:04.143Z"
        };

        _tokenCacheServiceMock.Setup(t => t.GetToken(request.Email))
            .Returns(token);

        _openSenseMapServiceMock.Setup(s => s.CreateNewSenseBoxAsync(request, token))
            .ReturnsAsync(expectedResponse);

        var result = await _controller.CreateNewSenseBox(request);

        var okResult = Assert.IsType<OkObjectResult>(result);
        var response = Assert.IsType<CreateBoxResponse>(okResult.Value);
        Assert.Equal("test-box-id", response.Id);
        Assert.Equal("Test Box", response.Name);
    }

    [Fact]
    public async Task CreateNewSenseBox_WithoutToken_ReturnsUnauthorized()
    {
        var request = new NewSenseBoxRequest
        {
            Email = "test@example.com",
            Name = "Test Box",
            Exposure = "indoor",
            Model = "homeV2Wifi",
            Location = new LocationDto
            {
                Lat = 90,
                Lng = 180,
                Height = 100
            }
        };

        _tokenCacheServiceMock.Setup(t => t.GetToken(request.Email))
            .Returns((string?)null);

        var result = await _controller.CreateNewSenseBox(request);

        var unauthorizedResult = Assert.IsType<UnauthorizedObjectResult>(result);
        var response = Assert.IsType<ApiResponse<object>>(unauthorizedResult.Value);
        Assert.False(response.Success);
        Assert.Contains("not authenticated", response.Message);
    }

    [Fact]
    public async Task CreateNewSenseBox_InvalidModelState_ReturnsBadRequest()
    {
        _controller.ModelState.AddModelError("Name", "Name is required");

        var request = new NewSenseBoxRequest
        {
            Email = "test@example.com",
            Exposure = "indoor",
            Model = "homeV2Wifi",
            Location = new LocationDto
            {
                Lat = 90,
                Lng = 180,
                Height = 100
            }
        };

        var result = await _controller.CreateNewSenseBox(request);

        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        var response = Assert.IsType<ApiResponse<object>>(badRequestResult.Value);
        Assert.False(response.Success);
        Assert.Equal("Validation failed", response.Message);
    }

    [Fact]
    public async Task GetSenseBoxById_ValidId_ReturnsOk()
    {
        var senseBoxId = "123456789";
        var expectedResponse = new GetSenseBoxResponse
        {
            Id = senseBoxId,
            Name = "Test Box",
            Exposure = "outdoor",
            CreatedAt = "2026-02-28T06:22:04.143Z",
            UpdatedAt = "2026-02-28T06:22:04.143Z",
            Grouptag = new List<string>(),
            Sensors = new List<SensorWithMeasurement>()
        };

        _openSenseMapServiceMock.Setup(s => s.GetSenseBoxByIdAsync(senseBoxId))
            .ReturnsAsync(expectedResponse);

        var result = await _controller.GetSenseBoxById(senseBoxId);

        var okResult = Assert.IsType<OkObjectResult>(result);
        var response = Assert.IsType<GetSenseBoxResponse>(okResult.Value);
        Assert.Equal(senseBoxId, response.Id);
        Assert.Equal("Test Box", response.Name);
    }

    [Fact]
    public async Task GetSenseBoxById_EmptyId_ReturnsBadRequest()
    {
        var result = await _controller.GetSenseBoxById("");

        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        var response = Assert.IsType<ApiResponse<object>>(badRequestResult.Value);
        Assert.False(response.Success);
        Assert.Equal("Sense box ID is required", response.Message);
    }

    [Fact]
    public async Task GetSenseBoxById_NullId_ReturnsBadRequest()
    {
        var result = await _controller.GetSenseBoxById(null!);

        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        var response = Assert.IsType<ApiResponse<object>>(badRequestResult.Value);
        Assert.False(response.Success);
    }
}
