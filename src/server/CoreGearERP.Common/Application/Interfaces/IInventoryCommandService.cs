namespace CoreGearERP.Common.Application.Interfaces;

/// <summary>
/// Defines the commands this module exposes to other modules.
/// Currently resolved in-process. Replaced with gRPC at M4.
/// </summary>
public interface IInventoryCommandService
{
    /// <summary>
    /// Adds stock to a StockItem. Called on goods receipt.
    /// </summary>
    Task AddStockAsync(
        Guid productId,
        Guid warehouseId,
        decimal quantity,
        string unitCode,
        Guid referenceId,
        string referenceNumber,
        Guid tenantId,
        Guid userId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Reserves stock against an open order.
    /// </summary>
    Task ReserveStockAsync(
        Guid productId,
        Guid warehouseId,
        decimal quantity,
        string unitCode,
        Guid tenantId,
        Guid userId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Releases a previously held reservation.
    /// </summary>
    Task ReleaseReservationAsync(
        Guid productId,
        Guid warehouseId,
        decimal quantity,
        string unitCode,
        Guid tenantId,
        Guid userId,
        CancellationToken cancellationToken = default);
}