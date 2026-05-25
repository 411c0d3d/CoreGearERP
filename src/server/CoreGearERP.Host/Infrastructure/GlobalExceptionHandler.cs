using CoreGearERP.Common.Domain.Exceptions;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace CoreGearERP.Host.Infrastructure;

/// <summary>
/// Global exception handler using the built-in IExceptionHandler interface.
/// Integrates correctly with the ASP.NET Core pipeline so Serilog sees the correct status code.
/// </summary>
public class GlobalExceptionHandler : IExceptionHandler
{
    private readonly ILogger<GlobalExceptionHandler> _logger;

    /// <summary>
    /// Initializes a new instance of the GlobalExceptionHandler class with the specified logger.
    /// </summary>
    /// <param name="logger">The logger to log exceptions.</param>
    public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger)
    {
        _logger = logger;
    }

    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        var (statusCode, message) = exception switch
        {
            DomainException ex => (HttpStatusCode.BadRequest, ex.Message),
            NotFoundException ex => (HttpStatusCode.NotFound, ex.Message),
            _ => (HttpStatusCode.InternalServerError, "An unexpected error occurred.")
        };

        if (statusCode == HttpStatusCode.InternalServerError)
        {
            _logger.LogError(exception, "Unhandled exception");
        }
        else
        {
            _logger.LogWarning("{Message}", message);
        }

        httpContext.Response.StatusCode = (int)statusCode;

        await httpContext.Response.WriteAsJsonAsync(
            new ProblemDetails
            {
                Status = (int)statusCode,
                Detail = message
            },
            cancellationToken);

        return true;
    }
}