namespace CoreGearERP.Common.Application.Interfaces;

/// <summary>
/// Marker interface for commands. Commands mutate state and return a result.
/// </summary>
public interface ICommand<TResult>
{
}

/// <summary>
/// Marker interface for commands that return no result.
/// </summary>
public interface ICommand : ICommand<Unit>
{
}