using FluentValidation;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace MeuSitePessoal.Api.Middleware;

// Handles all unhandled exceptions globally to provide a consistent JSON response.
public class GlobalExceptionHandler : IExceptionHandler
{
    private readonly ILogger<GlobalExceptionHandler> _logger;

    public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger)
    {
        _logger = logger;
    }

    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        if (exception is ValidationException validationException)
        {
            // Logging as Information or Warning since it's a client error, not a system failure.
            _logger.LogInformation("Validation failed for request {Path}", httpContext.Request.Path);
            
            var errors = validationException.Errors
                .GroupBy(e => e.PropertyName)
                .ToDictionary(
                    g => g.Key,
                    g => g.Select(e => e.ErrorMessage).ToArray()
                );

            var validationProblemDetails = new ValidationProblemDetails(errors)
            {
                Status = StatusCodes.Status400BadRequest,
                Title = "Validation Error",
                Detail = "One or more validation errors occurred.",
                Instance = httpContext.Request.Path
            };

            httpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
            await httpContext.Response.WriteAsJsonAsync(validationProblemDetails, cancellationToken);
            return true;
        }

        if (exception is KeyNotFoundException)
        {
            _logger.LogInformation("Resource not found: {Message}", exception.Message);
        }
        else
        {
            // For real system failures, maintain the LogError with the full stack trace.
            _logger.LogError(exception, "An unhandled exception occurred: {Message}", exception.Message);
        }
        
        // Maps specific domain exceptions to appropriate HTTP status codes.
        var (statusCode, title) = exception switch
        {
            ArgumentException => (StatusCodes.Status400BadRequest, "Invalid Request Data"),
            KeyNotFoundException => (StatusCodes.Status404NotFound, "Resource Not Found"),
            UnauthorizedAccessException => (StatusCodes.Status401Unauthorized, "Unauthorized Access"),
            _ => (StatusCodes.Status500InternalServerError, "Internal Server Error")
        };

        // Prepares the standardized problem details response following RFC 7807.
        var problemDetails = new ProblemDetails
        {
            Status = statusCode,
            Title = title,
            Detail = exception.Message,
            Instance = httpContext.Request.Path
        };

        httpContext.Response.StatusCode = statusCode;

        // Writes the problem details object as a JSON response to the client.
        await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);

        // Returns true to signal that the exception has been handled.
        return true;
    }
}