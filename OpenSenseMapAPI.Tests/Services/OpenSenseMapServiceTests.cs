using System.Net;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Protected;
using OpenSenseMapAPI.Exceptions;
using OpenSenseMapAPI.Models;
using OpenSenseMapAPI.Services;
using Xunit;

namespace OpenSenseMapAPI.Tests.Services;

public class OpenSenseMapServiceTests
{
    private readonly Mock<ILogger<OpenSenseMapService>> _loggerMock;
    private readonly Mock<IConfiguration> _configurationMock;
    private readonly Mock<HttpMessageHandler> _httpMessageHandlerMock;
    private readonly HttpClient _httpClient;
    private readonly OpenSenseMapService _service;

    public OpenSenseMapServiceTests()
    {
        _loggerMock = new Mock<ILogger<OpenSenseMapService>>();
        _configurationMock = new Mock<IConfiguration>();
        _httpMessageHandlerMock = new Mock<HttpMessageHandler>();
        
        _configurationMock.Setup(c => c["OpenSenseMap:BaseUrl"])
            .Returns("https://api.opensensemap.org");

        _httpClient = new HttpClient(_httpMessageHandlerMock.Object)
        {
            BaseAddress = new Uri("https://api.opensensemap.org")
        };

        _service = new OpenSenseMapService(_httpClient, _configurationMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task RegisterUserAsync_ValidRequest_ReturnsSuccess()
    {
        var request = new RegisterRequest
        {
            Name = "Test User",
            Email = "test@example.com",
            Password = "password123"
        };

        var responseContent = new RegisterResponse
        {
            Code = "created",
            Message = "Successfully registered new user.",
            Token = "test-token-123",
            Data = new object()
        };
        
        var jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };
        
        var httpResponse = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(JsonSerializer.Serialize(responseContent, jsonOptions), Encoding.UTF8, "application/json")
        };

        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(httpResponse);

        var result = await _service.RegisterUserAsync(request);

        Assert.NotNull(result);
        Assert.IsType<RegisterResponse>(result);
        var registerResponse = result as RegisterResponse;
        Assert.Equal("created", registerResponse?.Code);
        Assert.Equal("test-token-123", registerResponse?.Token);
    }

    [Fact]
    public async Task RegisterUserAsync_FailedRequest_ThrowsOpenSenseMapException()
    {
        var request = new RegisterRequest
        {
            Name = "Test User",
            Email = "test@example.com",
            Password = "password123"
        };

        var httpResponse = new HttpResponseMessage(HttpStatusCode.BadRequest)
        {
            Content = new StringContent("Registration failed", Encoding.UTF8, "application/json")
        };

        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(httpResponse);

        await Assert.ThrowsAsync<OpenSenseMapException>(() => _service.RegisterUserAsync(request));
    }

    [Fact]
    public async Task LoginAsync_ValidCredentials_ReturnsLoginResponse()
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
        
        var jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        var httpResponse = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(JsonSerializer.Serialize(loginResponse, jsonOptions), Encoding.UTF8, "application/json")
        };

        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(httpResponse);

        var result = await _service.LoginAsync(request);

        Assert.NotNull(result);
        Assert.Equal("test-token", result.Token);
        Assert.Equal("Authorized", result.Code);
        Assert.Equal("refresh-token", result.RefreshToken);
    }

    [Fact]
    public async Task LoginAsync_InvalidCredentials_ThrowsOpenSenseMapException()
    {
        var request = new LoginRequest
        {
            Email = "test@example.com",
            Password = "wrongpassword"
        };

        var httpResponse = new HttpResponseMessage(HttpStatusCode.Unauthorized)
        {
            Content = new StringContent("Invalid credentials", Encoding.UTF8, "application/json")
        };

        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(httpResponse);

        await Assert.ThrowsAsync<OpenSenseMapException>(() => _service.LoginAsync(request));
    }

    [Fact]
    public async Task CreateNewSenseBoxAsync_WithValidToken_ReturnsSuccess()
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
        var apiResponse = new CreateBoxApiResponse
        {
            Message = "Box successfully created",
            Data = new CreateBoxResponse
            {
                Id = "test-box-id",
                Name = "Test Box",
                Exposure = "indoor",
                Model = "homeV2Wifi",
                CreatedAt = "2026-02-28T06:22:04.143Z",
                UpdatedAt = "2026-02-28T06:22:04.143Z"
            }
        };
        
        var jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };
        
        var httpResponse = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(JsonSerializer.Serialize(apiResponse, jsonOptions), Encoding.UTF8, "application/json")
        };

        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(httpResponse);

        var result = await _service.CreateNewSenseBoxAsync(request, token);

        Assert.NotNull(result);
        Assert.IsType<CreateBoxResponse>(result);
        Assert.Equal("test-box-id", result.Id);
        Assert.Equal("Test Box", result.Name);
    }

    [Fact]
    public async Task CreateNewSenseBoxAsync_WithoutToken_ThrowsUnauthorizedAccessException()
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

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => 
            _service.CreateNewSenseBoxAsync(request, ""));
    }

    [Fact]
    public async Task GetSenseBoxByIdAsync_ValidId_ReturnsBox()
    {
        var senseBoxId = "123456789";
        var responseContent = new GetSenseBoxResponse
        {
            Id = senseBoxId,
            Name = "Test Box",
            Exposure = "outdoor",
            CreatedAt = "2026-02-28T06:22:04.143Z",
            UpdatedAt = "2026-02-28T06:22:04.143Z",
            Grouptag = new List<string>(),
            Sensors = new List<SensorWithMeasurement>()
        };
        
        var jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };
        
        var httpResponse = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(JsonSerializer.Serialize(responseContent, jsonOptions), Encoding.UTF8, "application/json")
        };

        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(httpResponse);

        var result = await _service.GetSenseBoxByIdAsync(senseBoxId);

        Assert.NotNull(result);
        Assert.IsType<GetSenseBoxResponse>(result);
        Assert.Equal(senseBoxId, result.Id);
        Assert.Equal("Test Box", result.Name);
    }

    [Fact]
    public async Task GetSenseBoxByIdAsync_EmptyId_ThrowsArgumentException()
    {
        await Assert.ThrowsAsync<ArgumentException>(() => _service.GetSenseBoxByIdAsync(""));
    }

    [Fact]
    public async Task GetSenseBoxByIdAsync_NotFound_ThrowsOpenSenseMapException()
    {
        var senseBoxId = "nonexistent";
        var httpResponse = new HttpResponseMessage(HttpStatusCode.NotFound)
        {
            Content = new StringContent("Box not found", Encoding.UTF8, "application/json")
        };

        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(httpResponse);

        await Assert.ThrowsAsync<OpenSenseMapException>(() => _service.GetSenseBoxByIdAsync(senseBoxId));
    }

    [Fact]
    public async Task LogoutAsync_WithValidToken_ReturnsSuccess()
    {
        var token = "valid-token";
        var responseContent = new { message = "Logged out successfully" };
        var httpResponse = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(JsonSerializer.Serialize(responseContent), Encoding.UTF8, "application/json")
        };

        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(httpResponse);

        var result = await _service.LogoutAsync(token);

        Assert.NotNull(result);
    }

    [Fact]
    public async Task LogoutAsync_WithoutToken_ThrowsUnauthorizedAccessException()
    {
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => _service.LogoutAsync(""));
    }
}
