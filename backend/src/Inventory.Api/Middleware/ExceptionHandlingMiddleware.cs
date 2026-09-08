using System.Net;
using System.Text.Json;
using Inventory.Application.Common;
using Inventory.Domain.Exceptions;

namespace Inventory.Api.Middleware;

public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
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

    private static async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";

        var response = exception switch
        {
            InsufficientStockException ex => new
            {
                StatusCode = HttpStatusCode.BadRequest,
                Body = ApiResponse<object>.Fail(ex.Code, ex.Message)
            },
            ValidationException ex => new
            {
                StatusCode = HttpStatusCode.BadRequest,
                Body = ApiResponse<object>.Fail(ex.Code, ex.Message, ex.Errors)
            },
            EntityNotFoundException ex => new
            {
                StatusCode = HttpStatusCode.NotFound,
                Body = ApiResponse<object>.Fail(ex.Code, ex.Message)
            },
            DomainException ex => new
            {
                StatusCode = HttpStatusCode.BadRequest,
                Body = ApiResponse<object>.Fail(ex.Code, ex.Message)
            },
            _ => new
            {
                StatusCode = HttpStatusCode.InternalServerError,
                Body = ApiResponse<object>.Fail("INTERNAL_SERVER_ERROR", "An unexpected error occurred on the server.")
            }
        };

        context.Response.StatusCode = (int)response.StatusCode;
        var json = JsonSerializer.Serialize(response.Body, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        await context.Response.WriteAsync(json);
    }
}
