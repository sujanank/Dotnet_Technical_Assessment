using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;
using OpenSenseMapAPI.Exceptions;
using OpenSenseMapAPI.Middleware;
using OpenSenseMapAPI.Models;
using Xunit;

namespace OpenSenseMapAPI.Tests.Middleware;

public class GlobalExceptionHandlerMiddlewareTests
{
    private readonly Mock<ILogger<GlobalExceptionHandlerMiddleware>> _loggerMock;
    private readonly DefaultHttpContext _httpContext;

    public GlobalExceptionHandlerMiddlewareTests()
    {
        _loggerMock = new Mock<ILogger<GlobalExceptionHandlerMiddleware>>();
        _httpContext = new DefaultHttpContext();
        _httpContext.Response.Body = new MemoryStream();
    }

    [Fact]
    public async Task InvokeAsync_NoException_CallsNextDelegate()
    {
        var nextCalled = false;
        RequestDelegate next = (HttpContext ctx) =>
        {
            nextCalled = true;
            return Task.CompletedTask;
        };

        var middleware = new GlobalExceptionHandlerMiddleware(next, _loggerMock.Object);

        await middleware.InvokeAsync(_httpContext);

        Assert.True(nextCalled);
    }

    [Fact]
    public async Task InvokeAsync_OpenSenseMapException_ReturnsCorrectStatusCode()
    {
        var exceptionMessage = "Test exception";
        var statusCode = 400;

        RequestDelegate next = (HttpContext ctx) =>
        {
            throw new OpenSenseMapException(exceptionMessage, statusCode);
        };

        var middleware = new GlobalExceptionHandlerMiddleware(next, _loggerMock.Object);

        await middleware.InvokeAsync(_httpContext);

        Assert.Equal(statusCode, _httpContext.Response.StatusCode);
        Assert.Equal("application/json", _httpContext.Response.ContentType);

        _httpContext.Response.Body.Seek(0, SeekOrigin.Begin);
        var responseBody = await new StreamReader(_httpContext.Response.Body).ReadToEndAsync();
        var response = JsonSerializer.Deserialize<ApiResponse<object>>(responseBody, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        Assert.NotNull(response);
        Assert.False(response.Success);
        Assert.Contains(exceptionMessage, response.Errors);
    }

    [Fact]
    public async Task InvokeAsync_UnauthorizedAccessException_Returns401()
    {
        RequestDelegate next = (HttpContext ctx) =>
        {
            throw new UnauthorizedAccessException("Unauthorized");
        };

        var middleware = new GlobalExceptionHandlerMiddleware(next, _loggerMock.Object);

        await middleware.InvokeAsync(_httpContext);

        Assert.Equal((int)HttpStatusCode.Unauthorized, _httpContext.Response.StatusCode);
    }

    [Fact]
    public async Task InvokeAsync_ArgumentNullException_Returns400()
    {
        RequestDelegate next = (HttpContext ctx) =>
        {
            throw new ArgumentNullException("testParam", "Parameter cannot be null");
        };

        var middleware = new GlobalExceptionHandlerMiddleware(next, _loggerMock.Object);

        await middleware.InvokeAsync(_httpContext);

        Assert.Equal((int)HttpStatusCode.BadRequest, _httpContext.Response.StatusCode);
    }

    [Fact]
    public async Task InvokeAsync_ArgumentException_Returns400()
    {
        RequestDelegate next = (HttpContext ctx) =>
        {
            throw new ArgumentException("Invalid argument");
        };

        var middleware = new GlobalExceptionHandlerMiddleware(next, _loggerMock.Object);

        await middleware.InvokeAsync(_httpContext);

        Assert.Equal((int)HttpStatusCode.BadRequest, _httpContext.Response.StatusCode);
    }

    [Fact]
    public async Task InvokeAsync_GenericException_Returns500()
    {
        RequestDelegate next = (HttpContext ctx) =>
        {
            throw new Exception("Unexpected error");
        };

        var middleware = new GlobalExceptionHandlerMiddleware(next, _loggerMock.Object);

        await middleware.InvokeAsync(_httpContext);

        Assert.Equal((int)HttpStatusCode.InternalServerError, _httpContext.Response.StatusCode);
    }

    [Fact]
    public async Task InvokeAsync_Exception_LogsError()
    {
        var exceptionMessage = "Test error";
        RequestDelegate next = (HttpContext ctx) =>
        {
            throw new Exception(exceptionMessage);
        };

        var middleware = new GlobalExceptionHandlerMiddleware(next, _loggerMock.Object);

        await middleware.InvokeAsync(_httpContext);

        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => true),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception?, string>>((v, t) => true)),
            Times.Once);
    }
}
