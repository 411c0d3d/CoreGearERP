using CoreGearERP.Common.Application.Interfaces;
using CoreGearERP.Inventory.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CoreGearERP.Inventory.Application.Contracts;

/// <summary>
/// In-process implementation of IInventoryQueryService.
/// Called by Production and Sales to query inventory state.
/// Replaced with gRPC service implementation at M4.
/// </summary>
public class InventoryQueryService : IInventoryQueryService
{
    private readonly InventoryDbContext _context;

    /// <summary>
    /// Constructor with dependencies injected. Used for querying inventory state, such as available stock and best warehouse for reservation.
    /// </summary>
    /// <param name="context">Database context for inventory.</param>
    public InventoryQueryService(InventoryDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Gets available stock for a product across all warehouses. Used by Sales when checking availability before reserving stock for an order line.
    /// </summary>
    /// <param name="productId">The ID of the product.</param>
    /// <param name="tenantId">The ID of the tenant.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The available stock quantity.</returns>
    public async Task<decimal> GetAvailableStockAsync(
        Guid productId,
        Guid tenantId,
        CancellationToken cancellationToken = default)
    {
        return await _context.StockItems
            .Where(s => s.ProductId == productId && s.TenantId == tenantId)
            .SumAsync(s => s.QuantityOnHand.Value - s.QuantityReserved.Value, cancellationToken);
    }

    /// <summary>
    /// Gets available stock for a product in a specific warehouse. Used by Sales when reserving stock for an order line.
    /// </summary>
    /// <param name="productId">The ID of the product.</param>
    /// <param name="warehouseId">The ID of the warehouse.</param>
    /// <param name="tenantId">The ID of the tenant.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The available stock quantity.</returns>
    public async Task<decimal> GetAvailableStockInWarehouseAsync(
        Guid productId,
        Guid warehouseId,
        Guid tenantId,
        CancellationToken cancellationToken = default)
    {
        var stockItem = await _context.StockItems
            .FirstOrDefaultAsync(s => s.ProductId == productId
                                      && s.WarehouseId == warehouseId
                                      && s.TenantId == tenantId,
                cancellationToken);

        return stockItem is null
            ? 0
            : stockItem.QuantityOnHand.Value - stockItem.QuantityReserved.Value;
    }
    
    /// <summary>
    /// Finds the best warehouse to fulfill a reservation for a given product and quantity. Used by Sales when reserving stock for an order line, to determine which warehouse to reserve from.
    /// </summary>
    /// <param name="productId">The ID of the product.</param>
    /// <param name="quantity">The quantity to reserve.</param>
    /// <param name="tenantId">The ID of the tenant.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The ID of the best warehouse.</returns>
    public async Task<Guid> FindBestWarehouseAsync(
        Guid productId,
        decimal quantity,
        Guid tenantId,
        CancellationToken cancellationToken = default)
    {
        var stockItem = await _context.StockItems
            .Where(s => s.ProductId == productId
                        && s.TenantId == tenantId
                        && (s.QuantityOnHand.Value - s.QuantityReserved.Value) >= quantity)
            .OrderByDescending(s => s.QuantityOnHand.Value - s.QuantityReserved.Value)
            .FirstOrDefaultAsync(cancellationToken);

        return stockItem?.WarehouseId ?? Guid.Empty;
    }
}