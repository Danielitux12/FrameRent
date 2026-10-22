using System.Text.Json;
using FrameRent.Application.Common;
using FrameRent.Domain.Exceptions;
using Microsoft.AspNetCore.Mvc;

namespace FrameRent.Api.Middleware;

/// <summary>
/// Global middleware capturing domain, application, and unhandled exceptions,
/// mapping them to RFC 7807 ProblemDetails HTTP responses.
/// </summary>
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
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var (statusCode, title, detail) = exception switch
        {
            DomainException domainEx =>
                (StatusCodes.Status400BadRequest, "Domain Business Rule Violation", domainEx.Message),

            ValidationAppException validationEx =>
                (StatusCodes.Status400BadRequest, "Validation Error", validationEx.Message),

            AuthenticationFailedException authEx =>
                (StatusCodes.Status401Unauthorized, "Authentication Failed", authEx.Message),

            ForbiddenAccessException forbiddenEx =>
                (StatusCodes.Status403Forbidden, "Forbidden Access", forbiddenEx.Message),

            NotFoundException notFoundEx =>
                (StatusCodes.Status404NotFound, "Resource Not Found", notFoundEx.Message),

            KeyNotFoundException keyNotFoundEx =>
                (StatusCodes.Status404NotFound, "Resource Not Found", keyNotFoundEx.Message),

            UnauthorizedAccessException unauthorizedEx =>
                (StatusCodes.Status401Unauthorized, "Unauthorized", unauthorizedEx.Message),

            _ => (StatusCodes.Status500InternalServerError, "Internal Server Error", "An unexpected error occurred while processing the request.")
        };

        if (statusCode == StatusCodes.Status500InternalServerError)
        {
            _logger.LogError(exception, "Unhandled exception occurred: {Message}", exception.Message);
        }
        else
        {
            _logger.LogWarning(exception, "Handled business/application exception ({StatusCode}): {Message}", statusCode, exception.Message);
        }

        var problemDetails = new ProblemDetails
        {
            Status = statusCode,
            Title = title,
            Detail = detail,
            Instance = context.Request.Path
        };

        context.Response.ContentType = "application/problem+json";
        context.Response.StatusCode = statusCode;

        var json = JsonSerializer.Serialize(problemDetails);
        await context.Response.WriteAsync(json);
    }
}
