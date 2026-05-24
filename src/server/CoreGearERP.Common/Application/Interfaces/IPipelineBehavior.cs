namespace CoreGearERP.Common.Application.Interfaces;

/// <summary>
/// Cross-cutting concern applied around every command and query.
/// Behaviors are executed in registration order.
/// Examples: validation, logging, performance tracking, retry.
/// </summary>
public interface IPipelineBehavior<TRequest, TResult>
{
    Task<TResult> Handle(
        TRequest request,
        Func<Task<TResult>> next,
        CancellationToken cancellationToken = default);
}