using CoreGearERP.Common.Application.Interfaces;
using CoreGearERP.Common.Domain.Exceptions;

namespace CoreGearERP.Host.Infrastructure;

/// <summary>
/// Resolves command and query handlers from DI and runs them through the pipeline.
/// Catches all exceptions internally and wraps them in Result to prevent propagation.
/// Serilog never sees exceptions -- response codes are always correct from the start.
/// </summary>
public class Dispatcher : IDispatcher
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<Dispatcher> _logger;

    public Dispatcher(IServiceProvider serviceProvider, ILogger<Dispatcher> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    /// <summary>
    /// Resolves and executes a command handler through the pipeline.
    /// </summary>
    public Task<Result<TResult>> SendCommand<TResult>(
        ICommand<TResult> command,
        CancellationToken cancellationToken = default)
    {
        var handlerType = typeof(ICommandHandler<,>)
            .MakeGenericType(command.GetType(), typeof(TResult));

        var handler = _serviceProvider.GetRequiredService(handlerType);

        Func<Task<TResult>> handlerCall = () =>
        {
            var method = handlerType.GetMethod(nameof(ICommandHandler<ICommand<TResult>, TResult>.Handle))!;
            return (Task<TResult>)method.Invoke(handler, [command, cancellationToken])!;
        };

        return RunPipeline(command, handlerCall, cancellationToken);
    }

    /// <summary>
    /// Resolves and executes a query handler through the pipeline.
    /// </summary>
    public Task<Result<TResult>> SendQuery<TResult>(
        IQuery<TResult> query,
        CancellationToken cancellationToken = default)
    {
        var handlerType = typeof(IQueryHandler<,>)
            .MakeGenericType(query.GetType(), typeof(TResult));

        var handler = _serviceProvider.GetRequiredService(handlerType);

        Func<Task<TResult>> handlerCall = () =>
        {
            var method = handlerType.GetMethod(nameof(IQueryHandler<IQuery<TResult>, TResult>.Handle))!;
            return (Task<TResult>)method.Invoke(handler, [query, cancellationToken])!;
        };

        return RunPipeline(query, handlerCall, cancellationToken);
    }

    private async Task<Result<TResult>> RunPipeline<TResult>(
        object request,
        Func<Task<TResult>> handler,
        CancellationToken cancellationToken)
    {
        try
        {
            var behaviorType = typeof(IPipelineBehavior<,>)
                .MakeGenericType(request.GetType(), typeof(TResult));

            var behaviors = _serviceProvider
                .GetServices(behaviorType)
                .Reverse()
                .ToList();

            // Wrap behaviors around the handler in reverse order
            // so the first registered behavior is the outermost wrapper.
            var pipeline = behaviors.Aggregate(handler, (next, behavior) =>
            {
                return () =>
                {
                    var method = behaviorType
                        .GetMethod(nameof(IPipelineBehavior<object, TResult>.Handle))!;
                    return (Task<TResult>)method.Invoke(behavior, [request, next, cancellationToken])!;
                };
            });

            var result = await pipeline();
            return Result<TResult>.Success(result);
        }
        catch (DomainException ex)
        {
            _logger.LogWarning("Domain rule violation: {Message}", ex.Message);
            return Result<TResult>.Failure(ex.Message);
        }
        catch (NotFoundException ex)
        {
            _logger.LogWarning("Not found: {Message}", ex.Message);
            return Result<TResult>.NotFound(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception in dispatcher for {RequestType}", request.GetType().Name);
            return Result<TResult>.Unexpected("An unexpected error occurred.");
        }
    }
}