namespace CoreGearERP.Common.Application.Interfaces;

/// <summary>
/// Handles a command and returns a result.
/// </summary>
public interface ICommandHandler<TCommand, TResult> where TCommand : ICommand<TResult>
{
    /// <summary>
    /// Handles the command and returns a result. The result can be a simple success/failure, or it can be a complex object with data.
    /// </summary>
    /// <param name="command">The command to handle.</param>
    /// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
    /// <returns>The result of handling the command.</returns>
    Task<TResult> Handle(TCommand command, CancellationToken cancellationToken = default);
}