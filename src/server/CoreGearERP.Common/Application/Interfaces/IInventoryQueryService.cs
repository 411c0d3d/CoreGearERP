namespace CoreGearERP.Common.Application.Interfaces;

/// <summary>
/// Defines the queries this module exposes to other modules.
/// Currently resolved in-process. Replaced with gRPC at M4.
/// Other modules depend on this interface, never on the implementation directly.
/// </summary>
public interface IInventoryQueryService
{
    /// <summary>Returns the available quantity for a product in a warehouse.</summary>
    Task<decimal> GetAvailableStockAsync(
        Guid productId,
        Guid warehouseId,
        Guid tenantId,
        CancellationToken cancellationToken = default);
}