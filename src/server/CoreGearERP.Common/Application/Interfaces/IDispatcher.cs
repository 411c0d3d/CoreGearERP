namespace CoreGearERP.Common.Application.Interfaces;

/// <summary>
/// Single entry point for all commands and queries.
/// Resolves the correct handler, runs the pipeline, and returns the result.
/// Replaces IMediator. Every endpoint and cross-module call goes through this.
/// </summary>
public interface IDispatcher
{
    Task<Result<TResult>> SendCommand<TResult>(ICommand<TResult> command,
        CancellationToken cancellationToken = default);

    Task<Result<TResult>> SendQuery<TResult>(IQuery<TResult> query, CancellationToken cancellationToken = default);
}