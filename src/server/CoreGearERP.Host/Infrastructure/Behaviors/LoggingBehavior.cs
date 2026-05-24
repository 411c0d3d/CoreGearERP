using CoreGearERP.Common.Application.Interfaces;
using CoreGearERP.Common.Domain.Exceptions;
using System.Diagnostics;

namespace CoreGearERP.Host.Infrastructure.Behaviors;

/// <summary>Logs every command and query with execution time. Warns on slow handlers.</summary>
public class LoggingBehavior<TRequest, TResult> : IPipelineBehavior<TRequest, TResult>
{
    private readonly ILogger<LoggingBehavior<TRequest, TResult>> _logger;

    /// <summary>
    /// Initializes a new instance of the LoggingBehavior class with the specified logger.
    /// </summary>
    /// <param name="logger">The logger to use for logging request handling.</param>
    public LoggingBehavior(ILogger<LoggingBehavior<TRequest, TResult>> logger)
    {
        _logger = logger;
    }

    public async Task<TResult> Handle(
        TRequest request,
        Func<Task<TResult>> next,
        CancellationToken cancellationToken = default)
    {
        var requestName = typeof(TRequest).Name;
        var stopwatch   = Stopwatch.StartNew();

        _logger.LogInformation("Handling {RequestName}", requestName);

        try
        {
            var result = await next();
            stopwatch.Stop();

            if (stopwatch.ElapsedMilliseconds > 500)
            {
                _logger.LogWarning(
                    "Slow handler: {RequestName} took {ElapsedMs}ms",
                    requestName,
                    stopwatch.ElapsedMilliseconds);
            }
            else
            {
                _logger.LogInformation(
                    "Handled {RequestName} in {ElapsedMs}ms",
                    requestName,
                    stopwatch.ElapsedMilliseconds);
            }

            return result;
        }
        catch (DomainException)
        {
            // DomainException is an expected business rule violation, not a system error.
            // Let it propagate -- ExceptionMiddleware handles it and returns 400.
            throw;
        }
        catch (NotFoundException)
        {
            // Same -- expected, not a system error.
            throw;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _logger.LogError(ex,
                "Unhandled error in {RequestName} after {ElapsedMs}ms",
                requestName,
                stopwatch.ElapsedMilliseconds);
            throw;
        }
    }
}