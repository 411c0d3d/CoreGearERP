namespace CoreGearERP.Common.Application.Interfaces;

/// <summary>
/// Defines the commands this module exposes to other modules.
/// All operations are explicit about which warehouse is involved.
/// Currently resolved in-process. Replaced with gRPC at M4.
/// </summary>
public interface IInventoryCommandService
{
    /// <summary>
    /// Adds stock to a specific warehouse on goods receipt.
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
    /// Adds finished goods stock from a completed production order.
    /// </summary>
    Task AddStockFromProductionAsync(
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
    /// Issues stock from a specific warehouse for production component consumption.
    /// </summary>
    Task IssueStockAsync(
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
    /// Reserves stock in a specific warehouse.
    /// </summary>
    Task ReserveStockInWarehouseAsync(
        Guid productId,
        Guid warehouseId,
        decimal quantity,
        string unitCode,
        Guid tenantId,
        Guid userId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Releases a previously held reservation in a specific warehouse.
    /// </summary>
    Task ReleaseReservationAsync(
        Guid productId,
        Guid warehouseId,
        decimal quantity,
        string unitCode,
        Guid tenantId,
        Guid userId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Removes stock from a specific warehouse on sales shipment. Creates SalesShipment movement.
    /// </summary>
    Task ShipStockAsync(
        Guid productId,
        Guid warehouseId,
        decimal quantity,
        string unitCode,
        Guid referenceId,
        string referenceNumber,
        Guid tenantId,
        Guid userId,
        CancellationToken cancellationToken = default);
}