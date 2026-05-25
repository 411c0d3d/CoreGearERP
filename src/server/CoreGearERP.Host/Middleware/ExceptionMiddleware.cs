using System.Net;
using CoreGearERP.Common.Domain.Exceptions;

namespace CoreGearERP.Host.Middleware;

/// <summary>
/// Global exception handler. Catches domain and infrastructure exceptions
/// and maps them to consistent HTTP responses.
/// Nothing else in the app should return raw exceptions to the client.
/// </summary>
public class ExceptionMiddleware
{
    private readonly RequestDelegate _next;

    private readonly ILogger<ExceptionMiddleware> _logger;
    
    /// <summary>
    /// Initializes a new instance of the <see cref="ExceptionMiddleware"/> class.
    /// </summary>
    /// <param name="next">The Next Request Delegate.</param>
    /// <param name="logger">The logger to log exceptions.</param>
    public ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    /// <summary>
    /// Invoke the middleware. Catches exceptions and maps them to HTTP responses.
    /// </summary>
    /// <param name="context">The HttpContext for the current request.</param>
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (DomainException ex)
        {
            // Expected business rule violation -- log as warning without stack trace.
            _logger.LogWarning("Domain rule violation: {Message}", ex.Message);
            context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
            await context.Response.WriteAsJsonAsync(new { error = ex.Message });
        }
        catch (NotFoundException ex)
        {
            // Expected -- log as warning without stack trace.
            _logger.LogWarning("Not found: {Message}", ex.Message);
            context.Response.StatusCode = (int)HttpStatusCode.NotFound;
            await context.Response.WriteAsJsonAsync(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception");
            context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
            await context.Response.WriteAsJsonAsync(new { error = "An unexpected error occurred." });
        }
    }
}