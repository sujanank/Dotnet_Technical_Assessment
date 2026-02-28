using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Text.Json;
using OpenSenseMapAPI.Exceptions;
using OpenSenseMapAPI.Models;

namespace OpenSenseMapAPI.Middleware;

public class GlobalExceptionHandlerMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionHandlerMiddleware> _logger;

    public GlobalExceptionHandlerMiddleware(RequestDelegate next, ILogger<GlobalExceptionHandlerMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unhandled exception occurred: {Message}", ex.Message);
            await HandleExceptionAsync(context, ex);
        }
    }

    private static Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var statusCode = HttpStatusCode.InternalServerError;
        var message = "An unexpected error occurred.";
        var errors = new List<string>();

        switch (exception)
        {
            case OpenSenseMapException openSenseMapEx:
                statusCode = (HttpStatusCode)openSenseMapEx.StatusCode;
                message = openSenseMapEx.Message;
                errors.Add(openSenseMapEx.Message);
                break;

            case ValidationException validationEx:
                statusCode = HttpStatusCode.BadRequest;
                message = "Validation failed.";
                errors.Add(validationEx.Message);
                break;

            case UnauthorizedAccessException:
                statusCode = HttpStatusCode.Unauthorized;
                message = "Unauthorized access.";
                errors.Add(exception.Message);
                break;

            case ArgumentNullException argNullEx:
                statusCode = HttpStatusCode.BadRequest;
                message = "Invalid argument provided.";
                errors.Add(argNullEx.Message);
                break;

            case ArgumentException argEx:
                statusCode = HttpStatusCode.BadRequest;
                message = "Invalid argument.";
                errors.Add(argEx.Message);
                break;

            default:
                errors.Add(exception.Message);
                break;
        }

        var response = ApiResponse<object>.FailureResponse(message, errors);

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)statusCode;

        var options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        return context.Response.WriteAsync(JsonSerializer.Serialize(response, options));
    }
}
