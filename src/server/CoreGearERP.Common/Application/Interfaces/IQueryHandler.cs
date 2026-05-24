namespace CoreGearERP.Common.Application.Interfaces;

/// <summary>
/// Handles a query and returns a result.
/// </summary>
public interface IQueryHandler<TQuery, TResult> where TQuery : IQuery<TResult>
{
    Task<TResult> Handle(TQuery query, CancellationToken cancellationToken = default);
}