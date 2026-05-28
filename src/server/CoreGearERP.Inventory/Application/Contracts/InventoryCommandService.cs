using CoreGearERP.Common.Application.Interfaces;
using CoreGearERP.Common.Domain.Exceptions;
using CoreGearERP.Common.Domain.ValueObjects;
using CoreGearERP.Inventory.Domain.Entities;
using CoreGearERP.Inventory.Domain.Enums;
using CoreGearERP.Inventory.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CoreGearERP.Inventory.Application.Contracts;

/// <summary>
/// In-process implementation of IInventoryCommandService.
/// All operations are explicit about which warehouse is involved.
/// Replaced with gRPC service implementation at M4.
/// </summary>
public class InventoryCommandService : IInventoryCommandService
{
    private readonly InventoryDbContext _context;

    /// <summary>
    /// Initializes a new instance of the InventoryCommandService with the specified InventoryDbContext.
    /// </summary>
    /// <param name="context">The InventoryDbContext used for database operations.</param>
    public InventoryCommandService(InventoryDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Adds stock to a specific warehouse on goods receipt.
    /// </summary>
    public async Task AddStockAsync(
        Guid productId,
        Guid warehouseId,
        decimal quantity,
        string unitCode,
        Guid referenceId,
        string referenceNumber,
        Guid tenantId,
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        var stockItem = await GetStockItemAsync(productId, warehouseId, tenantId, cancellationToken);

        var qty = new Quantity(quantity, unitCode);

        stockItem.AddStock(qty, userId);

        _context.StockMovements.Add(StockMovement.Create(
            stockItemId: stockItem.Id,
            productId: productId,
            warehouseId: warehouseId,
            movementType: StockMovementType.GoodsReceipt,
            quantity: qty,
            tenantId: tenantId,
            createdBy: userId,
            referenceId: referenceId,
            referenceNumber: referenceNumber));

        await _context.SaveChangesAsync(cancellationToken);
    }

    /// <summary>
    /// Adds finished goods stock from a completed production order.
    /// </summary>
    public async Task AddStockFromProductionAsync(
        Guid productId,
        Guid warehouseId,
        decimal quantity,
        string unitCode,
        Guid referenceId,
        string referenceNumber,
        Guid tenantId,
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        var stockItem = await GetStockItemAsync(productId, warehouseId, tenantId, cancellationToken);

        var qty = new Quantity(quantity, unitCode);
        stockItem.AddStock(qty, userId);

        _context.StockMovements.Add(StockMovement.Create(
            stockItemId: stockItem.Id,
            productId: productId,
            warehouseId: warehouseId,
            movementType: StockMovementType.ProductionReceipt,
            quantity: qty,
            tenantId: tenantId,
            createdBy: userId,
            referenceId: referenceId,
            referenceNumber: referenceNumber));

        await _context.SaveChangesAsync(cancellationToken);
    }

    /// <summary>
    /// Issues stock from a specific warehouse for production component consumption.
    /// </summary>
    public async Task IssueStockAsync(
        Guid productId,
        Guid warehouseId,
        decimal quantity,
        string unitCode,
        Guid referenceId,
        string referenceNumber,
        Guid tenantId,
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        var stockItem = await GetStockItemAsync(productId, warehouseId, tenantId, cancellationToken);

        var qty = new Quantity(quantity, unitCode);
        stockItem.RemoveStock(qty, userId);

        _context.StockMovements.Add(StockMovement.Create(
            stockItemId: stockItem.Id,
            productId: productId,
            warehouseId: warehouseId,
            movementType: StockMovementType.GoodsIssue,
            quantity: qty,
            tenantId: tenantId,
            createdBy: userId,
            referenceId: referenceId,
            referenceNumber: referenceNumber));

        await _context.SaveChangesAsync(cancellationToken);
    }

    /// <summary>
    /// Reserves stock in a specific warehouse.
    /// </summary>
    public async Task ReserveStockInWarehouseAsync(
        Guid productId,
        Guid warehouseId,
        decimal quantity,
        string unitCode,
        Guid tenantId,
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        var stockItem = await GetStockItemAsync(productId, warehouseId, tenantId, cancellationToken);
        stockItem.Reserve(new Quantity(quantity, unitCode), userId);
        await _context.SaveChangesAsync(cancellationToken);
    }

    /// <summary>
    /// Releases a previously held reservation in a specific warehouse.
    /// </summary>
    public async Task ReleaseReservationAsync(
        Guid productId,
        Guid warehouseId,
        decimal quantity,
        string unitCode,
        Guid tenantId,
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        var stockItem = await GetStockItemAsync(productId, warehouseId, tenantId, cancellationToken);
        stockItem.ReleaseReservation(new Quantity(quantity, unitCode), userId);
        await _context.SaveChangesAsync(cancellationToken);
    }

    private async Task<StockItem> GetStockItemAsync(
        Guid productId,
        Guid warehouseId,
        Guid tenantId,
        CancellationToken cancellationToken)
    {
        var stockItem = await _context.StockItems
            .FirstOrDefaultAsync(s => s.ProductId == productId
                                      && s.WarehouseId == warehouseId
                                      && s.TenantId == tenantId,
                cancellationToken);

        if (stockItem is null)
        {
            throw new NotFoundException("StockItem", $"Product {productId} in Warehouse {warehouseId}");
        }

        return stockItem;
    }

    /// <summary>
    /// Removes stock from a specific warehouse on sales shipment.
    /// </summary>
    public async Task ShipStockAsync(
        Guid productId,
        Guid warehouseId,
        decimal quantity,
        string unitCode,
        Guid referenceId,
        string referenceNumber,
        Guid tenantId,
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        var stockItem = await GetStockItemAsync(productId, warehouseId, tenantId, cancellationToken);

        var qty = new Quantity(quantity, unitCode);
        stockItem.RemoveStock(qty, userId);

        _context.StockMovements.Add(StockMovement.Create(
            stockItemId: stockItem.Id,
            productId: productId,
            warehouseId: warehouseId,
            movementType: StockMovementType.SalesShipment,
            quantity: qty,
            tenantId: tenantId,
            createdBy: userId,
            referenceId: referenceId,
            referenceNumber: referenceNumber));

        await _context.SaveChangesAsync(cancellationToken);
    }
}