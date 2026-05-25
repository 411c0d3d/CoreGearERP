namespace CoreGearERP.Common.Application.Interfaces;

/// <summary>
/// Defines the queries this module exposes to other modules.
/// Currently resolved in-process. Replaced with gRPC at M4.
/// Other modules depend on this interface, never on the implementation directly.
/// </summary>
public interface IInventoryQueryService
{
    /// <summary>
    /// Returns total available quantity for a product across all warehouses.
    /// </summary>
    Task<decimal> GetAvailableStockAsync(
        Guid productId,
        Guid tenantId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns available quantity for a product in a specific warehouse.
    /// </summary>
    Task<decimal> GetAvailableStockInWarehouseAsync(
        Guid productId,
        Guid warehouseId,
        Guid tenantId,
        CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Finds the warehouse with the most available stock for a product.
    /// Used as fallback when the caller does not specify an explicit warehouse.
    /// Returns Guid.Empty if no warehouse has sufficient stock.
    /// </summary>
    Task<Guid> FindBestWarehouseAsync(
        Guid productId,
        decimal quantity,
        Guid tenantId,
        CancellationToken cancellationToken = default);
}