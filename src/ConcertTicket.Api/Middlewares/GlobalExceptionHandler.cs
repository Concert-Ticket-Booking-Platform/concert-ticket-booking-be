using ConcertTicket.Api.Models;
using System.Net;

namespace ConcertTicket.Api.Middlewares;

public class GlobalExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionMiddleware> _logger;

    public GlobalExceptionMiddleware(RequestDelegate next, ILogger<GlobalExceptionMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext httpContext)
    {
        try
        {
            await _next(httpContext);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception occurred: {Message}", ex.Message);
            await HandleExceptionAsync(httpContext, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext httpContext, Exception exception)
    {
        var response = new ApiResponse<object>
        {
            Success = false,
            Message = exception.Message,
            Errors = new List<string> { exception.Message }
        };

        if (exception is UnauthorizedAccessException)
        {
            httpContext.Response.StatusCode = StatusCodes.Status401Unauthorized;
        }
        else if (exception is InvalidOperationException)
        {
            httpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
        }
        else if (exception is ArgumentException || exception is ArgumentNullException)
        {
            httpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
        }
        else
        {
            httpContext.Response.StatusCode = StatusCodes.Status500InternalServerError;
            response.Message = "An unexpected error occurred.";
        }

        httpContext.Response.ContentType = "application/json";
        await httpContext.Response.WriteAsJsonAsync(response);
    }
}
